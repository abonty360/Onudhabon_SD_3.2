using System.Text;
using System.Text.Json;

namespace Onudhabon_ISD.Services
{
    public class GeminiChatService : ILlmChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiChatService(IConfiguration configuration)
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };

            // Read from .env first, then appsettings.json
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? configuration["Gemini:ApiKey"]
                      ?? string.Empty;

            _model = Environment.GetEnvironmentVariable("GEMINI_MODEL")
                     ?? configuration["Gemini:Model"]
                     ?? "gemini-3.6-flash";
        }

        public async Task<string> GetChatResponseAsync(string userMessage, string? systemContext = null)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return "Gemini API key is not configured. Please add GEMINI_API_KEY in your .env file or appsettings.json.";
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var requestBody = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(systemContext))
            {
                requestBody["system_instruction"] = new
                {
                    parts = new[] { new { text = systemContext } }
                };
            }

            requestBody["contents"] = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = userMessage } }
                }
            };

            requestBody["generationConfig"] = new
            {
                temperature = 0.7,
                maxOutputTokens = 1024
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Gemini API Error ({response.StatusCode}): {responseString}";
                }

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var candidateContent) &&
                        candidateContent.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString() ?? "No response generated.";
                    }
                }

                return "No response received from Gemini.";
            }
            catch (Exception ex)
            {
                return $"Error connecting to Gemini API: {ex.Message}";
            }
        }
    }
}