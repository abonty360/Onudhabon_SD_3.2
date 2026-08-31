namespace Onudhabon_ISD.Services
{
    public interface IPdfKnowledgeService
    {
        string ExtractTextFromStream(Stream stream, int maxPages = 30);
        Task<string> ExtractTextFromUrlAsync(string url, int maxPages = 30);
        Task<string> GetGroundingContextForQueryAsync(string userQuery, string? attachedPdfText = null);
        string ExtractRelevantExcerpt(string fullText, string query, int maxChars = 4000);
    }
}
