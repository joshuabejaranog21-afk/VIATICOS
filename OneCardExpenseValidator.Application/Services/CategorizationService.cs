using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneCardExpenseValidator.Application.DTOs;
using OneCardExpenseValidator.Infrastructure.Data;

namespace OneCardExpenseValidator.Application.Services
{
    /// <summary>
    /// Servicio de categorización de productos usando búsqueda por keywords y Claude Vision API
    /// Implementa una estrategia de fallback: primero keywords locales, luego Claude AI
    /// </summary>
    public interface ICategorizationService
    {
        Task<ValidationResponseDto> AnalyzeProductImageAsync(string imageBase64, string? description = null);
        Task<TicketAnalysisResponseDto> AnalyzeFullTicketAsync(string imageBase64);
        Task<ProductCategorizationDto> AnalyzeProductByNameAsync(string productName);
    }

    public class CategorizationService : ICategorizationService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CategorizationService> _logger;
        private readonly HttpClient _httpClient;

        // Configuración de Claude API
        private readonly string _claudeApiKey;
        private readonly string _claudeApiUrl = "https://api.anthropic.com/v1/messages";
        private readonly string _claudeModel = "claude-3-haiku-20240307";

        public CategorizationService(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<CategorizationService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();

            // Obtener API key desde configuración
            _claudeApiKey = _configuration["Claude:ApiKey"] ?? throw new InvalidOperationException("Claude API Key no configurada en appsettings.json");
        }

        /// <summary>
        /// Analiza una imagen de producto y determina si es deducible
        /// 1. Intenta búsqueda por keywords en BD local
        /// 2. Si falla, usa Claude Vision API
        /// 3. Retorna resultado estructurado
        /// </summary>
        public async Task<ValidationResponseDto> AnalyzeProductImageAsync(string imageBase64, string? description = null)
        {
            try
            {
                _logger.LogInformation("Iniciando análisis de producto. Descripción: {Description}", description ?? "Sin descripción");

                // PASO 1: Intentar búsqueda por keywords si hay descripción
                if (!string.IsNullOrWhiteSpace(description))
                {
                    var keywordResult = await TryAnalyzeByKeywordsAsync(description);
                    if (keywordResult != null)
                    {
                        _logger.LogInformation("Producto categorizado por keywords: {ProductName}", keywordResult.ProductName);
                        return keywordResult;
                    }
                }

                // PASO 2: Usar Claude Vision API
                _logger.LogInformation("Usando Claude Vision API para análisis de imagen");
                var claudeResult = await AnalyzeWithClaudeVisionAsync(imageBase64, description);

                if (claudeResult != null)
                {
                    _logger.LogInformation("Producto categorizado por Claude: {ProductName}", claudeResult.ProductName);
                    return claudeResult;
                }

                // PASO 3: Fallback - resultado genérico que requiere revisión manual
                _logger.LogWarning("No se pudo categorizar el producto, retornando resultado genérico");
                return CreateManualReviewResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en análisis de producto");

                // Retornar resultado de error que requiere revisión manual
                return new ValidationResponseDto
                {
                    ProductName = "Error en análisis",
                    Category = "Desconocido",
                    IsDeductible = false,
                    Confidence = 0.0,
                    Reason = $"Error al procesar la imagen: {ex.Message}",
                    AnalysisMethod = "Error",
                    RequiresManualReview = true,
                    AdditionalNotes = "Por favor, intente nuevamente o realice una revisión manual."
                };
            }
        }

        /// <summary>
        /// Intenta categorizar usando keywords de la base de datos
        /// </summary>
        private async Task<ValidationResponseDto?> TryAnalyzeByKeywordsAsync(string description)
        {
            try
            {
                var descriptionUpper = description.ToUpper();

                // Buscar en CategoryKeywords
                var matchedKeyword = await _context.CategoryKeywords
                    .Include(ck => ck.Category)
                    .Where(ck => ck.IsActive == true && descriptionUpper.Contains(ck.Keyword.ToUpper()))
                    .OrderByDescending(ck => ck.Weight)
                    .FirstOrDefaultAsync();

                if (matchedKeyword != null && matchedKeyword.Category != null)
                {
                    var category = matchedKeyword.Category;

                    // Determinar si es deducible según la categoría
                    var isDeductible = IsDeductibleCategory(category.CategoryCode, category.CategoryName);

                    return new ValidationResponseDto
                    {
                        ProductName = description,
                        Category = category.CategoryName,
                        IsDeductible = isDeductible.isDeductible,
                        Confidence = 0.85, // Keywords tienen alta confianza
                        Reason = isDeductible.reason,
                        AnalysisMethod = "Keywords",
                        RequiresManualReview = false
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda por keywords");
                return null;
            }
        }

        /// <summary>
        /// Analiza la imagen usando Claude Vision API
        /// </summary>
        private async Task<ValidationResponseDto?> AnalyzeWithClaudeVisionAsync(string imageBase64, string? description)
        {
            try
            {
                // Limpiar el prefijo "data:image/..." si existe
                var cleanBase64 = CleanBase64Image(imageBase64);

                // Construir el prompt para Claude
                var prompt = BuildClaudePrompt(description);

                // Construir el request para Claude API
                var requestBody = new
                {
                    model = _claudeModel,
                    max_tokens = 1024,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = "image",
                                    source = new
                                    {
                                        type = "base64",
                                        media_type = "image/jpeg",
                                        data = cleanBase64
                                    }
                                },
                                new
                                {
                                    type = "text",
                                    text = prompt
                                }
                            }
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

                _logger.LogInformation("Enviando request a Claude API");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Claude API retornó error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return null;
                }

                _logger.LogInformation("Respuesta de Claude recibida exitosamente");

                // Parsear la respuesta de Claude
                var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseContent);

                if (claudeResponse?.Content != null && claudeResponse.Content.Length > 0)
                {
                    var textContent = claudeResponse.Content[0].Text;

                    // Intentar extraer JSON de la respuesta
                    if (!string.IsNullOrWhiteSpace(textContent))
                    {
                        var visionResult = ExtractJsonFromClaudeResponse(textContent);

                        if (visionResult != null)
                        {
                            return new ValidationResponseDto
                            {
                                ProductName = visionResult.ProductName,
                                Category = visionResult.Category,
                                IsDeductible = visionResult.IsDeductible,
                                Confidence = visionResult.Confidence,
                                Reason = visionResult.Reason,
                                AnalysisMethod = "Claude",
                                RequiresManualReview = visionResult.Confidence < 0.7,
                                AdditionalNotes = visionResult.Subcategory != null ? $"Subcategoría: {visionResult.Subcategory}" : null
                            };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a Claude Vision API");
                return null;
            }
        }

        /// <summary>
        /// Construye el prompt para Claude con las políticas empresariales
        /// </summary>
        private string BuildClaudePrompt(string? description)
        {
            var prompt = @"Analiza este producto y determina si es un gasto deducible según las siguientes políticas empresariales:

GASTOS DEDUCIBLES:
- Agua embotellada y café básico
- Material de oficina (papelería, folders, etc.)
- Tecnología para trabajo (hasta $5,000 MXN): cables, cargadores, memorias USB, teclados, mouse
- Transporte relacionado con trabajo (Uber, taxis, gasolina)
- Comidas de negocios (no restaurantes de lujo)
- Papelería y suministros de oficina

GASTOS NO DEDUCIBLES:
- Alcohol de cualquier tipo
- Restaurantes caros o de lujo
- Entretenimiento personal (cine, videojuegos, etc.)
- Artículos de lujo o personales
- Tacos, hamburguesas y comida rápida (salvo comidas de negocios formales)
- Snacks y dulces
- Tecnología costosa (>$5,000 MXN)

Responde ÚNICAMENTE con un objeto JSON en el siguiente formato:
{
  ""productName"": ""nombre del producto identificado"",
  ""category"": ""categoría principal (Alimentos, Tecnología, Transporte, Oficina, etc.)"",
  ""subcategory"": ""subcategoría opcional"",
  ""isDeductible"": true o false,
  ""confidence"": 0.0 a 1.0,
  ""reason"": ""explicación clara de por qué es o no deducible"",
  ""estimatedPrice"": null o precio estimado si es visible
}";

            if (!string.IsNullOrWhiteSpace(description))
            {
                prompt += $"\n\nDescripción adicional proporcionada por el usuario: {description}";
            }

            return prompt;
        }

        /// <summary>
        /// Extrae y parsea el JSON de la respuesta de Claude
        /// </summary>
        private ClaudeVisionResponseDto? ExtractJsonFromClaudeResponse(string responseText)
        {
            try
            {
                // Claude puede retornar JSON directamente o dentro de texto
                // Intentar encontrar el JSON entre llaves
                var startIndex = responseText.IndexOf('{');
                var endIndex = responseText.LastIndexOf('}');

                if (startIndex >= 0 && endIndex > startIndex)
                {
                    var jsonText = responseText.Substring(startIndex, endIndex - startIndex + 1);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    return JsonSerializer.Deserialize<ClaudeVisionResponseDto>(jsonText, options);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta JSON de Claude");
                return null;
            }
        }

        /// <summary>
        /// Limpia la cadena Base64 removiendo prefijos de data URI
        /// </summary>
        private string CleanBase64Image(string base64String)
        {
            if (base64String.Contains(","))
            {
                // Remover el prefijo "data:image/jpeg;base64," o similar
                return base64String.Split(',')[1];
            }
            return base64String;
        }

        /// <summary>
        /// Determina si una categoría es deducible según políticas empresariales
        /// </summary>
        private (bool isDeductible, string reason) IsDeductibleCategory(string categoryCode, string categoryName)
        {
            // Categorías deducibles
            var deductibleCategories = new Dictionary<string, string>
            {
                { "TEC", "Tecnología para uso laboral es deducible" },
                { "OFI", "Material de oficina es deducible" },
                { "TRA", "Transporte relacionado con trabajo es deducible" },
                { "BEB", "Bebidas básicas (agua, café) son deducibles" }
            };

            // Categorías no deducibles
            var nonDeductibleCategories = new Dictionary<string, string>
            {
                { "ALI", "Alimentos y comida rápida no son deducibles salvo comidas de negocios" },
                { "ENT", "Entretenimiento personal no es deducible" },
                { "LUX", "Artículos de lujo no son deducibles" },
                { "ALC", "Bebidas alcohólicas no son deducibles" }
            };

            if (deductibleCategories.TryGetValue(categoryCode, out var deductibleReason))
            {
                return (true, deductibleReason);
            }

            if (nonDeductibleCategories.TryGetValue(categoryCode, out var nonDeductibleReason))
            {
                return (false, nonDeductibleReason);
            }

            // Default: requiere revisión
            return (false, $"La categoría {categoryName} requiere revisión manual para determinar deducibilidad");
        }

        /// <summary>
        /// Crea una respuesta que requiere revisión manual
        /// </summary>
        private ValidationResponseDto CreateManualReviewResponse()
        {
            return new ValidationResponseDto
            {
                ProductName = "Producto no identificado",
                Category = "Desconocido",
                IsDeductible = false,
                Confidence = 0.0,
                Reason = "No se pudo identificar el producto automáticamente",
                AnalysisMethod = "Manual",
                RequiresManualReview = true,
                AdditionalNotes = "Este gasto requiere revisión manual por un administrador."
            };
        }

        #region Claude API Response Models

        /// <summary>
        /// Modelo para la respuesta de Claude API
        /// </summary>
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

        #endregion

        /// <summary>
        /// Analiza un recibo completo y extrae todos los productos con su estado de deducibilidad
        /// </summary>
        public async Task<TicketAnalysisResponseDto> AnalyzeFullTicketAsync(string imageBase64)
        {
            try
            {
                _logger.LogInformation("Iniciando análisis de recibo completo con Claude Vision");

                // Limpiar el prefijo "data:image/..." si existe
                var cleanBase64 = CleanBase64Image(imageBase64);

                // Construir el prompt para analizar el recibo completo
                var prompt = BuildFullTicketPrompt();

                // Construir el request para Claude API
                var requestBody = new
                {
                    model = _claudeModel,
                    max_tokens = 2048, // Aumentamos tokens para recibir lista completa de productos
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = "image",
                                    source = new
                                    {
                                        type = "base64",
                                        media_type = "image/jpeg",
                                        data = cleanBase64
                                    }
                                },
                                new
                                {
                                    type = "text",
                                    text = prompt
                                }
                            }
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

                _logger.LogInformation("Enviando request a Claude API para análisis completo de recibo");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Claude API retornó error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new TicketAnalysisResponseDto
                    {
                        Success = false,
                        Message = "Error al procesar el recibo con Claude AI"
                    };
                }

                _logger.LogInformation("Respuesta de Claude recibida exitosamente");

                // Parsear la respuesta de Claude
                var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseContent);

                if (claudeResponse?.Content != null && claudeResponse.Content.Length > 0)
                {
                    var textContent = claudeResponse.Content[0].Text;

                    // Intentar extraer JSON de la respuesta
                    if (!string.IsNullOrWhiteSpace(textContent))
                    {
                        var ticketAnalysis = ExtractTicketAnalysisFromResponse(textContent);

                        if (ticketAnalysis != null)
                        {
                            ticketAnalysis.Success = true;
                            ticketAnalysis.Message = $"Recibo procesado exitosamente. {ticketAnalysis.Products?.Count ?? 0} productos encontrados.";
                            return ticketAnalysis;
                        }
                    }
                }

                return new TicketAnalysisResponseDto
                {
                    Success = false,
                    Message = "No se pudo procesar el recibo correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar recibo completo");
                return new TicketAnalysisResponseDto
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Construye el prompt para analizar el recibo completo
        /// </summary>
        private string BuildFullTicketPrompt()
        {
            return @"Analiza este recibo/ticket completo y extrae TODOS los productos con su información.

Para cada producto, determina si es DEDUCIBLE o NO DEDUCIBLE según las siguientes políticas empresariales:

GASTOS DEDUCIBLES:
- Agua embotellada y café básico
- Material de oficina (papelería, folders, etc.)
- Tecnología para trabajo (hasta $5,000 MXN): cables, cargadores, memorias USB, teclados, mouse
- Transporte relacionado con trabajo (Uber, taxis, gasolina)
- Comidas de negocios (no restaurantes de lujo)
- Papelería y suministros de oficina

GASTOS NO DEDUCIBLES:
- Alcohol de cualquier tipo
- Restaurantes caros o de lujo
- Entretenimiento personal (cine, videojuegos, etc.)
- Artículos de lujo o personales
- Tacos, hamburguesas y comida rápida (salvo comidas de negocios formales)
- Snacks y dulces
- Tecnología costosa (>$5,000 MXN)

Extrae la siguiente información del recibo:

1. Información general del recibo (vendor, fecha, total)
2. Lista COMPLETA de todos los productos con:
   - Nombre del producto (tal como aparece en el recibo)
   - Cantidad
   - Precio unitario
   - Total
   - Categoría
   - Si es deducible o no
   - Razón de por qué es o no deducible

Responde ÚNICAMENTE con un objeto JSON en el siguiente formato:
{
  ""vendor"": ""nombre del vendedor/tienda"",
  ""ticketDate"": ""fecha en formato YYYY-MM-DD"",
  ""totalAmount"": total del recibo como número,
  ""products"": [
    {
      ""name"": ""nombre del producto como aparece en el recibo"",
      ""quantity"": cantidad como número,
      ""unitPrice"": precio unitario como número,
      ""totalPrice"": precio total como número,
      ""category"": ""categoría del producto"",
      ""isDeductible"": true o false,
      ""reason"": ""explicación breve de por qué es o no deducible"",
      ""confidence"": confianza de 0.0 a 1.0
    }
  ]
}

IMPORTANTE: Extrae TODOS los productos del recibo, no solo algunos. Si hay 10 productos, deben aparecer los 10 en el JSON.";
        }

        /// <summary>
        /// Extrae y parsea el análisis del ticket de la respuesta de Claude
        /// </summary>
        private TicketAnalysisResponseDto? ExtractTicketAnalysisFromResponse(string responseText)
        {
            try
            {
                // Claude puede retornar JSON directamente o dentro de texto
                // Intentar encontrar el JSON entre llaves
                var startIndex = responseText.IndexOf('{');
                var endIndex = responseText.LastIndexOf('}');

                if (startIndex >= 0 && endIndex > startIndex)
                {
                    var jsonText = responseText.Substring(startIndex, endIndex - startIndex + 1);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    return JsonSerializer.Deserialize<TicketAnalysisResponseDto>(jsonText, options);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al parsear análisis de ticket de Claude");
                return null;
            }
        }

        /// <summary>
        /// Analiza un producto individual por nombre usando Claude AI
        /// </summary>
        public async Task<ProductCategorizationDto> AnalyzeProductByNameAsync(string productName)
        {
            try
            {
                _logger.LogInformation($"Analizando producto por nombre con Claude AI: {productName}");

                // Construir el prompt para Claude
                var prompt = BuildProductCategorizationPrompt(productName);

                // Crear request para Claude API
                var requestBody = new
                {
                    model = _claudeModel,
                    max_tokens = 500,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Headers requeridos
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _claudeApiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                // Hacer request
                var response = await _httpClient.PostAsync(_claudeApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Claude API retornó error: {response.StatusCode} - {responseContent}");
                    return new ProductCategorizationDto
                    {
                        Success = false,
                        ProductName = productName,
                        Message = "Error al analizar el producto con Claude AI"
                    };
                }

                // Parsear respuesta
                var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseContent);
                var textContent = claudeResponse?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;

                if (string.IsNullOrEmpty(textContent))
                {
                    return new ProductCategorizationDto
                    {
                        Success = false,
                        ProductName = productName,
                        Message = "Claude no retornó información del producto"
                    };
                }

                // Parsear el JSON de respuesta
                var result = ParseProductCategorizationFromResponse(textContent, productName);

                if (result != null)
                {
                    result.Success = true;
                    result.Message = "Producto categorizado exitosamente con Claude AI";
                    return result;
                }

                return new ProductCategorizationDto
                {
                    Success = false,
                    ProductName = productName,
                    Message = "No se pudo parsear la respuesta de Claude"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al analizar producto con Claude: {productName}");
                return new ProductCategorizationDto
                {
                    Success = false,
                    ProductName = productName,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Construye el prompt para categorizar un producto por nombre
        /// </summary>
        private string BuildProductCategorizationPrompt(string productName)
        {
            return $@"Eres un experto en categorización de productos y gastos empresariales en México.

Analiza el siguiente producto y determina:
1. Su categoría (Alimentos, Tecnología, Transporte, Oficina, etc.)
2. Si es deducible como gasto empresarial en México
3. La razón de por qué es o no deducible
4. Tu nivel de confianza (0.0 a 1.0)

Producto: {productName}

Considera que:
- Alimentos básicos y bebidas alcohólicas generalmente NO son deducibles
- Equipo de oficina, tecnología para trabajo, transporte empresarial SÍ son deducibles
- Software, servicios empresariales, capacitación SÍ son deducibles
- Productos de lujo o uso personal generalmente NO son deducibles

Responde ÚNICAMENTE con un objeto JSON en el siguiente formato:
{{
  ""category"": ""categoría del producto"",
  ""isDeductible"": true o false,
  ""reason"": ""explicación breve de por qué es o no deducible"",
  ""confidence"": nivel de confianza de 0.0 a 1.0
}}";
        }

        /// <summary>
        /// Parsea la respuesta de Claude para categorización de producto
        /// </summary>
        private ProductCategorizationDto? ParseProductCategorizationFromResponse(string responseText, string productName)
        {
            try
            {
                // Buscar JSON en la respuesta
                var startIndex = responseText.IndexOf('{');
                var endIndex = responseText.LastIndexOf('}');

                if (startIndex >= 0 && endIndex > startIndex)
                {
                    var jsonText = responseText.Substring(startIndex, endIndex - startIndex + 1);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var parsed = JsonSerializer.Deserialize<ProductCategorizationResponse>(jsonText, options);

                    if (parsed != null)
                    {
                        return new ProductCategorizationDto
                        {
                            ProductName = productName,
                            Category = parsed.Category ?? "Sin categoría",
                            IsDeductible = parsed.IsDeductible,
                            Reason = parsed.Reason ?? "Sin razón especificada",
                            Confidence = parsed.Confidence
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al parsear categorización de producto");
                return null;
            }
        }

        /// <summary>
        /// Clase auxiliar para parsear la respuesta de categorización
        /// </summary>
        private class ProductCategorizationResponse
        {
            [JsonPropertyName("category")]
            public string? Category { get; set; }

            [JsonPropertyName("isDeductible")]
            public bool IsDeductible { get; set; }

            [JsonPropertyName("reason")]
            public string? Reason { get; set; }

            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
        }
    }
}
