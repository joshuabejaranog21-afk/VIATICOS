using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.API.Services;
using OneCardExpenseValidator.Infrastructure.Data;
using System.Text.Json;

namespace OneCardExpenseValidator.API.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class ProductScannerController : ControllerBase
{
    private readonly IOcrService _ocrService;
    private readonly IClaudeService _claudeService;
    private readonly AppDbContext _context;
    private readonly ILogger<ProductScannerController> _logger;

    public ProductScannerController(
        IOcrService ocrService,
        IClaudeService claudeService,
        AppDbContext context,
        ILogger<ProductScannerController> logger)
    {
        _ocrService = ocrService;
        _claudeService = claudeService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Escanea un producto desde imagen y verifica si es deducible
    /// </summary>
    [HttpPost("scan-product")]
    public async Task<ActionResult<ProductScanResult>> ScanProduct(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("No se ha proporcionado ninguna imagen");
        }

        try
        {
            _logger.LogInformation($"Scanning product from image: {image.FileName}");

            // Extraer texto con OCR
            string ocrText;
            using (var stream = image.OpenReadStream())
            {
                ocrText = await _ocrService.ExtractTextFromStreamAsync(stream);
            }

            if (string.IsNullOrWhiteSpace(ocrText))
            {
                return Ok(new ProductScanResult
                {
                    Success = false,
                    Message = "No se pudo leer el producto de la imagen"
                });
            }

            // Obtener categorías deducibles de la base de datos
            var categories = await _context.Categories
                .Select(c => new { c.CategoryName, c.Description, c.IsDeductible })
                .ToListAsync();

            var deductibleCategories = categories
                .Where(c => c.IsDeductible == true)
                .Select(c => c.CategoryName)
                .ToList();

            var nonDeductibleCategories = categories
                .Where(c => c.IsDeductible == false)
                .Select(c => c.CategoryName)
                .ToList();

            // Obtener políticas de negocio
            var policies = await _context.BusinessPolicies
                .Where(p => p.IsActive == true)
                .Select(p => new { p.PolicyName, p.MaxDailyAmount, p.MaxMonthlyAmount })
                .ToListAsync();

            // Crear prompt para Claude
            var prompt = $@"Analiza el siguiente texto extraído de un producto y determina:
1. ¿Qué producto es?
2. ¿Es deducible como gasto de empresa?
3. ¿A qué categoría pertenece?
4. ¿Hay restricciones o consideraciones?

Texto del producto: {ocrText}

Categorías DEDUCIBLES disponibles: {string.Join(", ", deductibleCategories)}
Categorías NO DEDUCIBLES: {string.Join(", ", nonDeductibleCategories)}

Políticas de la empresa:
{string.Join("\n", policies.Select(p => $"- {p.PolicyName}: Máximo diario ${p.MaxDailyAmount}, Máximo mensual ${p.MaxMonthlyAmount}"))}

Responde ÚNICAMENTE en formato JSON con esta estructura:
{{
    ""productName"": ""nombre del producto"",
    ""isDeductible"": true/false,
    ""category"": ""categoría correspondiente"",
    ""reason"": ""explicación breve de por qué es o no deducible"",
    ""estimatedAmount"": precio estimado si lo detectas (o null),
    ""restrictions"": ""restricciones o consideraciones importantes"",
    ""confidence"": número del 0 al 100 indicando qué tan seguro estás
}}";

            // Llamar a Claude
            var claudeResponse = await _claudeService.GenerateResponseAsync(prompt);

            // Parsear respuesta JSON de Claude
            ProductAnalysis? analysis;
            try
            {
                // Limpiar la respuesta si viene con markdown
                var jsonResponse = claudeResponse.Trim();
                if (jsonResponse.StartsWith("```json"))
                {
                    jsonResponse = jsonResponse.Substring(7);
                    jsonResponse = jsonResponse.Substring(0, jsonResponse.LastIndexOf("```"));
                }
                else if (jsonResponse.StartsWith("```"))
                {
                    jsonResponse = jsonResponse.Substring(3);
                    jsonResponse = jsonResponse.Substring(0, jsonResponse.LastIndexOf("```"));
                }

                analysis = JsonSerializer.Deserialize<ProductAnalysis>(jsonResponse.Trim(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Claude response: {Response}", claudeResponse);
                return Ok(new ProductScanResult
                {
                    Success = false,
                    Message = "Error al procesar la respuesta del análisis",
                    RawOcrText = ocrText,
                    ClaudeResponse = claudeResponse
                });
            }

            if (analysis == null)
            {
                return Ok(new ProductScanResult
                {
                    Success = false,
                    Message = "No se pudo analizar el producto",
                    RawOcrText = ocrText
                });
            }

            return Ok(new ProductScanResult
            {
                Success = true,
                Message = analysis.IsDeductible
                    ? $"✓ {analysis.ProductName} ES DEDUCIBLE"
                    : $"✗ {analysis.ProductName} NO es deducible",
                RawOcrText = ocrText,
                ProductName = analysis.ProductName,
                IsDeductible = analysis.IsDeductible,
                Category = analysis.Category,
                Reason = analysis.Reason,
                EstimatedAmount = analysis.EstimatedAmount,
                Restrictions = analysis.Restrictions,
                Confidence = analysis.Confidence
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning product");
            return StatusCode(500, new ProductScanResult
            {
                Success = false,
                Message = $"Error al escanear el producto: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Verifica si un texto de producto es deducible (sin imagen)
    /// </summary>
    [HttpPost("check-product")]
    public async Task<ActionResult<ProductScanResult>> CheckProduct([FromBody] CheckProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductText))
        {
            return BadRequest("No se ha proporcionado texto del producto");
        }

        try
        {
            // Reutilizar la misma lógica pero sin OCR
            var categories = await _context.Categories
                .Select(c => new { c.CategoryName, c.Description, c.IsDeductible })
                .ToListAsync();

            var deductibleCategories = categories
                .Where(c => c.IsDeductible == true)
                .Select(c => c.CategoryName)
                .ToList();

            var prompt = $@"Analiza el siguiente producto y determina si es deducible:

Producto: {request.ProductText}

Categorías DEDUCIBLES: {string.Join(", ", deductibleCategories)}

Responde ÚNICAMENTE en formato JSON con esta estructura:
{{
    ""productName"": ""nombre del producto"",
    ""isDeductible"": true/false,
    ""category"": ""categoría"",
    ""reason"": ""explicación breve"",
    ""confidence"": número del 0 al 100
}}";

            var claudeResponse = await _claudeService.GenerateResponseAsync(prompt);

            // Parsear respuesta
            var jsonResponse = claudeResponse.Trim();
            if (jsonResponse.StartsWith("```json"))
            {
                jsonResponse = jsonResponse.Substring(7);
                jsonResponse = jsonResponse.Substring(0, jsonResponse.LastIndexOf("```"));
            }

            var analysis = JsonSerializer.Deserialize<ProductAnalysis>(jsonResponse.Trim(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (analysis == null)
            {
                return Ok(new ProductScanResult
                {
                    Success = false,
                    Message = "No se pudo analizar el producto"
                });
            }

            return Ok(new ProductScanResult
            {
                Success = true,
                Message = analysis.IsDeductible
                    ? $"✓ {analysis.ProductName} ES DEDUCIBLE"
                    : $"✗ {analysis.ProductName} NO es deducible",
                ProductName = analysis.ProductName,
                IsDeductible = analysis.IsDeductible,
                Category = analysis.Category,
                Reason = analysis.Reason,
                Confidence = analysis.Confidence
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking product");
            return StatusCode(500, new ProductScanResult
            {
                Success = false,
                Message = $"Error al verificar el producto: {ex.Message}"
            });
        }
    }
}

public class ProductScanResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RawOcrText { get; set; }
    public string? ClaudeResponse { get; set; }
    public string? ProductName { get; set; }
    public bool IsDeductible { get; set; }
    public string? Category { get; set; }
    public string? Reason { get; set; }
    public decimal? EstimatedAmount { get; set; }
    public string? Restrictions { get; set; }
    public int Confidence { get; set; }
}

public class CheckProductRequest
{
    public string ProductText { get; set; } = string.Empty;
}

public class ProductAnalysis
{
    public string ProductName { get; set; } = string.Empty;
    public bool IsDeductible { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public decimal? EstimatedAmount { get; set; }
    public string? Restrictions { get; set; }
    public int Confidence { get; set; }
}
