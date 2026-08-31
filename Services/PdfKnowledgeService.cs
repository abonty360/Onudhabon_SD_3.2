using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;
using UglyToad.PdfPig;

namespace Onudhabon_ISD.Services
{
    public class PdfKnowledgeService : IPdfKnowledgeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PdfKnowledgeService> _logger;

        public PdfKnowledgeService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IServiceProvider serviceProvider,
            ILogger<PdfKnowledgeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public string ExtractTextFromStream(Stream stream, int maxPages = 30)
        {
            try
            {
                using var document = PdfDocument.Open(stream);
                var sb = new StringBuilder();

                var pages = document.GetPages().Take(maxPages);
                foreach (var page in pages)
                {
                    var pageText = page.Text;
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        sb.AppendLine($"[Page {page.Number}]");
                        sb.AppendLine(pageText.Trim());
                        sb.AppendLine();
                    }
                }

                return sb.ToString().Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract text from PDF stream.");
                return string.Empty;
            }
        }

        public async Task<string> ExtractTextFromUrlAsync(string url, int maxPages = 30)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var cacheKey = $"pdf_content_{url.Trim().ToLowerInvariant()}";
            if (_cache.TryGetValue(cacheKey, out string? cachedText) && cachedText != null)
            {
                return cachedText;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(25);

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Could not fetch PDF from URL {Url}: HTTP {StatusCode}", url, response.StatusCode);
                    return string.Empty;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                var text = ExtractTextFromStream(stream, maxPages);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Cache extracted text for 6 hours
                    _cache.Set(cacheKey, text, TimeSpan.FromHours(6));
                }

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error downloading and extracting PDF from {Url}", url);
                return string.Empty;
            }
        }

        public string ExtractRelevantExcerpt(string fullText, string query, int maxChars = 4000)
        {
            if (string.IsNullOrWhiteSpace(fullText))
                return string.Empty;

            if (fullText.Length <= maxChars)
                return fullText;

            // Extract keywords from query
            var queryWords = Regex.Matches(query, @"\w{3,}")
                .Select(m => m.Value.ToLowerInvariant())
                .Distinct()
                .ToList();

            if (!queryWords.Any())
            {
                // Return first chunk if no specific keywords
                return fullText.Substring(0, Math.Min(fullText.Length, maxChars)) + "\n... [Content truncated]";
            }

            // Split into paragraphs / sections
            var paragraphs = fullText.Split(new[] { "\r\n\r\n", "\n\n", "[Page " }, StringSplitOptions.RemoveEmptyEntries);
            var scoredParagraphs = new List<(string text, int score, int index)>();

            for (int i = 0; i < paragraphs.Length; i++)
            {
                var p = paragraphs[i].Trim();
                if (p.Length < 20) continue;

                int score = 0;
                var lowerP = p.ToLowerInvariant();
                foreach (var kw in queryWords)
                {
                    if (lowerP.Contains(kw))
                    {
                        score += 3;
                    }
                }

                if (score > 0)
                {
                    scoredParagraphs.Add((p, score, i));
                }
            }

            if (!scoredParagraphs.Any())
            {
                return fullText.Substring(0, Math.Min(fullText.Length, maxChars)) + "\n... [Content truncated]";
            }

            var bestParagraphs = scoredParagraphs
                .OrderByDescending(sp => sp.score)
                .ThenBy(sp => sp.index)
                .Take(6)
                .OrderBy(sp => sp.index)
                .Select(sp => sp.text);

            var combined = string.Join("\n\n---\n\n", bestParagraphs);
            if (combined.Length > maxChars)
            {
                combined = combined.Substring(0, maxChars) + "\n... [Excerpt truncated for length]";
            }

            return combined;
        }

        public async Task<string> GetGroundingContextForQueryAsync(string userQuery, string? attachedPdfText = null)
        {
            var sb = new StringBuilder();

            // 1. Attached PDF from User Chat Session
            if (!string.IsNullOrWhiteSpace(attachedPdfText))
            {
                sb.AppendLine("=== CURRENTLY ATTACHED PDF DOCUMENT ===");
                var relevantDocExcerpt = ExtractRelevantExcerpt(attachedPdfText, userQuery, 5000);
                sb.AppendLine(relevantDocExcerpt);
                sb.AppendLine("======================================");
                sb.AppendLine();
            }

            // 2. Search Relevant Platform Study Material PDFs
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var materials = await dbContext.Materials
                    .Where(m => m.Status == "Active" && !string.IsNullOrEmpty(m.FileUrl))
                    .OrderByDescending(m => m.Date)
                    .Take(10)
                    .ToListAsync();

                // Check which materials match query keywords
                var queryLower = userQuery.ToLowerInvariant();
                var matchingMaterials = materials
                    .Where(m =>
                        (!string.IsNullOrEmpty(m.Subject) && queryLower.Contains(m.Subject.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(m.Topic) && queryLower.Contains(m.Topic.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(m.Title) && queryLower.Contains(m.Title.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(m.ClassLevel) && queryLower.Contains(m.ClassLevel.ToLowerInvariant())) ||
                        queryLower.Contains("pdf") || queryLower.Contains("material") || queryLower.Contains("note") || queryLower.Contains("study"))
                    .Take(3)
                    .ToList();

                if (matchingMaterials.Any())
                {
                    sb.AppendLine("=== RELEVANT PLATFORM STUDY MATERIALS (PDFs) ===");
                    foreach (var mat in matchingMaterials)
                    {
                        sb.AppendLine($"Document: {mat.Title} | Subject: {mat.Subject} | Topic: {mat.Topic} | Class: {mat.ClassLevel}");
                        if (!string.IsNullOrEmpty(mat.FileUrl) && mat.FileUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            var pdfText = await ExtractTextFromUrlAsync(mat.FileUrl, 15);
                            if (!string.IsNullOrWhiteSpace(pdfText))
                            {
                                var excerpt = ExtractRelevantExcerpt(pdfText, userQuery, 2500);
                                sb.AppendLine($"Content from {mat.Title}:");
                                sb.AppendLine(excerpt);
                            }
                        }
                        sb.AppendLine();
                    }
                    sb.AppendLine("================================================");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error looking up platform study material PDFs.");
            }

            return sb.ToString();
        }
    }
}
