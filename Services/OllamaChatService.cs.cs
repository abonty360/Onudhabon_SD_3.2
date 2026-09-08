
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Onudhabon_ISD.Services
{
    public class OllamaChatService : ILlmChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;

        public OllamaChatService(IConfiguration configuration)
        {
            var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            _model = configuration["Ollama:Model"] ?? "llama3.2:3b";
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromMinutes(2)
            };
        }

        public async Task<string> GetChatResponseAsync(string userMessage, string? systemContext = null)
        {
            var messages = new List<object>();

            // 1. Add System Instructions / Context
            if (!string.IsNullOrWhiteSpace(systemContext))
            {
                messages.Add(new { role = "system", content = systemContext });
            }
            else
            {
                messages.Add(new
                {
                    role = "system",
                    content = "You are Onudhabon AI Assistant, an educational chatbot for the Onudhabon platform.Be helpful, clear, and support both English and Bengali."
                });
            }

            // 2. Add User Message
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = _model,
                messages = messages,
                stream = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/chat", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);

                var reply = doc.RootElement
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return reply ?? "Sorry, I could not generate a response.";
            }
            catch (Exception ex)
            {
                return $"Error connecting to AI service: {ex.Message}. Please make sure Ollama is running.";
            }
        }
    }
}