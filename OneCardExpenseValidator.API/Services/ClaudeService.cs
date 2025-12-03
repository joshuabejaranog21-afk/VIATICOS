using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OneCardExpenseValidator.API.Services
{
    public class ClaudeService : IClaudeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClaudeService> _logger;
        private readonly string _claudeApiKey;
        private readonly string _claudeModel;
        private readonly string _claudeApiUrl = "https://api.anthropic.com/v1/messages";

        public ClaudeService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ClaudeService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;

            // Obtener configuración de Claude
            _claudeApiKey = _configuration["Claude:ApiKey"]
                ?? throw new InvalidOperationException("Claude API Key no configurada. Verifica User Secrets o appsettings.json");
            _claudeModel = _configuration["Claude:Model"] ?? "claude-3-haiku-20240307";

            _logger.LogInformation("ClaudeService inicializado. Modelo: {Model}", _claudeModel);
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            try
            {
                _logger.LogInformation("Generando respuesta con Claude...");

                // Construir el request para Claude API
                var requestBody = new
                {
                    model = _claudeModel,
                    max_tokens = _configuration.GetValue<int>("Claude:MaxTokens", 1024),
                    temperature = _configuration.GetValue<double>("Claude:Temperature", 0.7),
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);

                var request = new HttpRequestMessage(HttpMethod.Post, _claudeApiUrl)
                {
                    Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
                };

                // Headers requeridos por Claude API
                request.Headers.Add("x-api-key", _claudeApiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Claude API retornó error: {StatusCode} - {Content}",
                        response.StatusCode, responseContent);

                    // Verificar si es un error de API key inválida
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("API Key de Claude inválida o expirada. Verifica tu configuración.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new Exception($"Endpoint de Claude no encontrado (404). Verifica la URL: {_claudeApiUrl}");
                    }

                    throw new Exception($"Error al llamar a Claude API: {response.StatusCode} - {responseContent}");
                }

                _logger.LogInformation("Respuesta de Claude recibida exitosamente");

                // Parsear la respuesta de Claude
                var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseContent);

                if (claudeResponse?.Content != null && claudeResponse.Content.Length > 0)
                {
                    return claudeResponse.Content[0].Text ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar respuesta con Claude");
                throw;
            }
        }

        private class ClaudeApiResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("content")]
            public ContentItem[]? Content { get; set; }

            [JsonPropertyName("model")]
            public string? Model { get; set; }

            [JsonPropertyName("stop_reason")]
            public string? StopReason { get; set; }
        }

        private class ContentItem
        {
            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}
