namespace Onudhabon_ISD.Services
{
    public interface ILlmChatService
    {
        Task<string> GetChatResponseAsync(string userMessage, string? systemContext = null);
    }
}