using System.Text;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using UglyToad.PdfPig;

namespace Onudhabon_ISD.Services
{
    public class PdfKnowledgeService : IPdfKnowledgeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PdfKnowledgeService> _logger;

        public PdfKnowledgeService(ApplicationDbContext context, ILogger<PdfKnowledgeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string ExtractTextFromStream(Stream stream, int maxPages = 40)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            try
            {
                using var memoryStream = new MemoryStream();
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                var sb = new StringBuilder();
                using (var document = PdfDocument.Open(memoryStream))
                {
                    int totalPages = document.NumberOfPages;
                    int pagesToRead = Math.Min(totalPages, maxPages);

                    for (int pageNumber = 1; pageNumber <= pagesToRead; pageNumber++)
                    {
                        var page = document.GetPage(pageNumber);
                        var text = page.Text;
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            sb.AppendLine($"--- Page {pageNumber} ---");
                            sb.AppendLine(text.Trim());
                            sb.AppendLine();
                        }
                    }

                    if (totalPages > maxPages)
                    {
                        sb.AppendLine($"\n[Note: Document truncated to the first {maxPages} of {totalPages} pages.]");
                    }
                }

                return sb.ToString().Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text from PDF stream.");
                throw;
            }
        }

        public async Task<string> GetGroundingContextForQueryAsync(string userQuery, string? attachedPdfText)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(attachedPdfText))
            {
                sb.AppendLine("=== ATTACHED PDF DOCUMENT KNOWLEDGE ===");
                if (attachedPdfText.Length > 60000)
                {
                    sb.AppendLine(attachedPdfText.Substring(0, 60000));
                    sb.AppendLine("\n[Note: Attached document content truncated to first 60,000 characters for token optimization]");
                }
                else
                {
                    sb.AppendLine(attachedPdfText.Trim());
                }
                sb.AppendLine("=== END ATTACHED PDF DOCUMENT ===");
            }

            // Grounding with relevant platform study materials if applicable
            try
            {
                if (!string.IsNullOrWhiteSpace(userQuery))
                {
                    var queryWords = userQuery.Split(new[] { ' ', ',', '.', '?', '!', ';', ':', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(w => w.Length > 2)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(5)
                        .ToList();

                    if (queryWords.Any())
                    {
                        var recentMaterials = await _context.Materials
                            .AsNoTracking()
                            .OrderByDescending(m => m.Date)
                            .Take(25)
                            .Select(m => new
                            {
                                m.Title,
                                m.Subject,
                                m.Topic,
                                m.ClassLevel,
                                m.FileUrl
                            })
                            .ToListAsync();

                        var matchingMaterials = recentMaterials
                            .Where(m => queryWords.Any(w =>
                                (!string.IsNullOrEmpty(m.Title) && m.Title.Contains(w, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrEmpty(m.Subject) && m.Subject.Contains(w, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrEmpty(m.Topic) && m.Topic.Contains(w, StringComparison.OrdinalIgnoreCase))))
                            .Take(3)
                            .ToList();

                        if (matchingMaterials.Any())
                        {
                            if (sb.Length > 0)
                            {
                                sb.AppendLine();
                            }
                            sb.AppendLine("PLATFORM STUDY MATERIALS RELEVANT TO USER'S QUERY:");
                            foreach (var mat in matchingMaterials)
                            {
                                sb.AppendLine($"- Title: {mat.Title} | Subject: {mat.Subject ?? "General"} | Class: {mat.ClassLevel ?? "N/A"} | Topic: {mat.Topic ?? "N/A"}");
                                if (!string.IsNullOrWhiteSpace(mat.FileUrl))
                                {
                                    sb.AppendLine($"  Link: {mat.FileUrl}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch platform study materials for grounding context.");
            }

            return sb.ToString().Trim();
        }
    }
}
