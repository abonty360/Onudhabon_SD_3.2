using System.Text;
using System.Text.Json;

namespace Onudhabon.Services
{
    public class GeminiChatService : ILlmChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly string _model;

        private static string NormalizeModelName(string? model)
        {
            if (string.IsNullOrWhiteSpace(model)) return "gemini-flash-lite-latest";
            var trimmed = model.Trim();
            if (trimmed.StartsWith("models/", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring("models/".Length);
            }
            return trimmed;
        }

        public GeminiChatService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            // Read from .env first, then appsettings.json
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? configuration["Gemini:ApiKey"]
                      ?? string.Empty;

            var configuredModel = Environment.GetEnvironmentVariable("GEMINI_MODEL")
                                  ?? configuration["Gemini:Model"];

            _model = NormalizeModelName(configuredModel);
        }

        public async Task<string> GetChatResponseAsync(string userMessage, string? systemContext = null)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return "Gemini API key is not configured. Please set GEMINI_API_KEY in your .env file or appsettings.json.";
            }

            var result = await SendGenerateContentRequestAsync(_model, userMessage, systemContext);
            if (result.IsSuccess)
            {
                return result.Text;
            }

            // If the primary model failed (404 Not Found, 503 Overloaded, or 429 Rate Limit),
            // swiftly try modern high-speed fallback models
            var statusCode = result.StatusCode;
            bool shouldFallback = statusCode == System.Net.HttpStatusCode.NotFound ||
                                  statusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                                  statusCode == (System.Net.HttpStatusCode)429;

            if (shouldFallback)
            {
                var fallbacks = new[] { "gemini-flash-lite-latest", "gemini-3.5-flash-lite", "gemini-3.8-flash" };
                foreach (var fallbackModel in fallbacks)
                {
                    if (string.Equals(fallbackModel, _model, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var fallbackResult = await SendGenerateContentRequestAsync(fallbackModel, userMessage, systemContext);
                    if (fallbackResult.IsSuccess)
                    {
                        return fallbackResult.Text;
                    }
                }
            }

            return result.ErrorMessage;
        }

        private async Task<(bool IsSuccess, string Text, string ErrorMessage, System.Net.HttpStatusCode? StatusCode)> SendGenerateContentRequestAsync(
            string modelName, string userMessage, string? systemContext)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_apiKey}";

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
                temperature = 0.2,
                maxOutputTokens = 1200,
                topP = 0.95
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(20);

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string extractedError = ParseErrorMessage(responseString, response.StatusCode);
                    return (false, string.Empty, extractedError, response.StatusCode);
                }

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var candidateContent) &&
                        candidateContent.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].GetProperty("text").GetString() ?? "No response generated.";
                        return (true, text, string.Empty, response.StatusCode);
                    }
                }

                return (false, string.Empty, "No response received from Gemini.", response.StatusCode);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, $"Error connecting to Gemini API: {ex.Message}", null);
            }
        }

        private static string ParseErrorMessage(string responseString, System.Net.HttpStatusCode statusCode)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("error", out var errorObj) &&
                    errorObj.TryGetProperty("message", out var msgElement))
                {
                    var msg = msgElement.GetString();
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        return $"Gemini API Error ({statusCode}): {msg}";
                    }
                }
            }
            catch
            {
                // Ignore parse errors and fallback
            }

            return $"Gemini API Error ({statusCode}): {responseString}";
        }
    }
}
