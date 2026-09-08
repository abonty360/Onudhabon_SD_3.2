namespace Onudhabon_ISD.Services
{
    public interface IPdfKnowledgeService
    {
        string ExtractTextFromStream(Stream stream, int maxPages = 40);
        Task<string> GetGroundingContextForQueryAsync(string userQuery, string? attachedPdfText);
    }
}
