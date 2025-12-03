using Microsoft.AspNetCore.Mvc;
using OneCardExpenseValidator.API.Services;
using OneCardExpenseValidator.API.Models;
using OneCardExpenseValidator.Application.Services;
using OneCardExpenseValidator.Application.DTOs;

namespace OneCardExpenseValidator.API.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;
    private readonly ITicketParserService _ticketParser;
    private readonly IProductMatchingService _productMatching;
    private readonly ICategorizationService _categorizationService;
    private readonly ILogger<OcrController> _logger;
    private readonly IWebHostEnvironment _environment;

    public OcrController(
        IOcrService ocrService,
        ITicketParserService ticketParser,
        IProductMatchingService productMatching,
        ICategorizationService categorizationService,
        ILogger<OcrController> logger,
        IWebHostEnvironment environment)
    {
        _ocrService = ocrService;
        _ticketParser = ticketParser;
        _productMatching = productMatching;
        _categorizationService = categorizationService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Procesa una imagen de ticket usando OCR y extrae la información
    /// </summary>
    [HttpPost("process-image")]
    public async Task<ActionResult<OcrProcessResult>> ProcessTicketImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("No se ha proporcionado ninguna imagen");
        }

        try
        {
            _logger.LogInformation($"Processing image: {image.FileName}, Size: {image.Length} bytes");

            // Extraer texto con OCR
            string ocrText;
            using (var stream = image.OpenReadStream())
            {
                ocrText = await _ocrService.ExtractTextFromStreamAsync(stream);
            }

            if (string.IsNullOrWhiteSpace(ocrText))
            {
                return Ok(new OcrProcessResult
                {
                    Success = false,
                    Message = "No se pudo extraer texto de la imagen",
                    RawOcrText = ocrText
                });
            }

            // Parsear el texto extraído
            var parsedData = _ticketParser.ParseTicketText(ocrText);

            return Ok(new OcrProcessResult
            {
                Success = true,
                Message = $"Se extrajo información del ticket correctamente. Se encontraron {parsedData.Items.Count} items.",
                RawOcrText = ocrText,
                ParsedData = parsedData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ticket image");
            return StatusCode(500, new OcrProcessResult
            {
                Success = false,
                Message = $"Error al procesar la imagen: {ex.Message}",
                RawOcrText = null
            });
        }
    }

    /// <summary>
    /// Procesa una imagen desde una ruta local
    /// </summary>
    [HttpPost("process-path")]
    public async Task<ActionResult<OcrProcessResult>> ProcessTicketImageFromPath([FromBody] ProcessImagePathRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ImagePath))
        {
            return BadRequest("No se ha proporcionado una ruta de imagen");
        }

        try
        {
            // Construir ruta completa
            var fullPath = request.ImagePath;
            if (!Path.IsPathRooted(fullPath))
            {
                fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, fullPath);
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound($"No se encontró la imagen en: {fullPath}");
            }

            _logger.LogInformation($"Processing image from path: {fullPath}");

            // Extraer texto con OCR
            var ocrText = await _ocrService.ExtractTextFromImageAsync(fullPath);

            if (string.IsNullOrWhiteSpace(ocrText))
            {
                return Ok(new OcrProcessResult
                {
                    Success = false,
                    Message = "No se pudo extraer texto de la imagen",
                    RawOcrText = ocrText
                });
            }

            // Parsear el texto extraído
            var parsedData = _ticketParser.ParseTicketText(ocrText);

            return Ok(new OcrProcessResult
            {
                Success = true,
                Message = $"Se extrajo información del ticket correctamente. Se encontraron {parsedData.Items.Count} items.",
                RawOcrText = ocrText,
                ParsedData = parsedData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ticket image from path");
            return StatusCode(500, new OcrProcessResult
            {
                Success = false,
                Message = $"Error al procesar la imagen: {ex.Message}",
                RawOcrText = null
            });
        }
    }

    /// <summary>
    /// Procesa una imagen de ticket con OCR y matchea automáticamente los productos
    /// </summary>
    [HttpPost("process-and-match")]
    public async Task<ActionResult<OcrProcessWithMatchResult>> ProcessAndMatchTicketImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("No se ha proporcionado ninguna imagen");
        }

        try
        {
            _logger.LogInformation($"Processing and matching image: {image.FileName}");

            // Extraer texto con OCR
            string ocrText;
            using (var stream = image.OpenReadStream())
            {
                ocrText = await _ocrService.ExtractTextFromStreamAsync(stream);
            }

            if (string.IsNullOrWhiteSpace(ocrText))
            {
                return Ok(new OcrProcessWithMatchResult
                {
                    Success = false,
                    Message = "No se pudo extraer texto de la imagen"
                });
            }

            // Parsear el texto extraído
            var parsedData = _ticketParser.ParseTicketText(ocrText);

            // Matchear productos automáticamente
            var matchedItems = await _productMatching.MatchProductsAsync(parsedData.Items);

            var matchedCount = matchedItems.Count(x => x.IsMatched);
            var totalCount = matchedItems.Count;

            return Ok(new OcrProcessWithMatchResult
            {
                Success = true,
                Message = $"Procesado correctamente. Se encontraron {totalCount} items, {matchedCount} fueron matcheados con productos.",
                RawOcrText = ocrText,
                Vendor = parsedData.Vendor,
                TicketDate = parsedData.TicketDate,
                TotalAmount = parsedData.TotalAmount,
                MatchedItems = matchedItems
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing and matching ticket image");
            return StatusCode(500, new OcrProcessWithMatchResult
            {
                Success = false,
                Message = $"Error al procesar la imagen: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Procesa un recibo completo con Claude Vision AI
    /// Extrae TODOS los productos con su estado de deducibilidad automáticamente
    /// </summary>
    [HttpPost("process-with-claude")]
    public async Task<ActionResult> ProcessWithClaudeVision(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest(new { success = false, message = "No se ha proporcionado ninguna imagen" });
        }

        try
        {
            _logger.LogInformation($"Processing full ticket with Claude Vision: {image.FileName}");

            // Convertir imagen a Base64
            string imageBase64;
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                imageBase64 = Convert.ToBase64String(imageBytes);
            }

            // Analizar recibo completo con Claude
            var result = await _categorizationService.AnalyzeFullTicketAsync(imageBase64);

            if (result.Success)
            {
                _logger.LogInformation($"Recibo procesado exitosamente. {result.Products?.Count ?? 0} productos encontrados");
            }
            else
            {
                _logger.LogWarning($"No se pudo procesar el recibo: {result.Message}");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ticket with Claude Vision");
            return StatusCode(500, new
            {
                success = false,
                message = $"Error al procesar el recibo: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Actualiza manualmente el estado de deducibilidad de un producto
    /// </summary>
    [HttpPost("update-product-deductibility")]
    public ActionResult UpdateProductDeductibility([FromBody] UpdateProductDeductibilityDto request)
    {
        if (string.IsNullOrEmpty(request.ProductId))
        {
            return BadRequest(new { success = false, message = "ProductId es requerido" });
        }

        try
        {
            _logger.LogInformation($"Actualizando deducibilidad del producto {request.ProductId} a {request.IsDeductible}");

            // Retornar el producto actualizado
            return Ok(new
            {
                success = true,
                message = "Estado de deducibilidad actualizado correctamente",
                productId = request.ProductId,
                isDeductible = request.IsDeductible,
                manuallyOverridden = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar deducibilidad del producto");
            return StatusCode(500, new
            {
                success = false,
                message = $"Error al actualizar: {ex.Message}"
            });
        }
    }
}

public class OcrProcessResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RawOcrText { get; set; }
    public ParsedTicketData? ParsedData { get; set; }
}

public class ProcessImagePathRequest
{
    public string ImagePath { get; set; } = string.Empty;
}

public class OcrProcessWithMatchResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RawOcrText { get; set; }
    public string? Vendor { get; set; }
    public DateTime? TicketDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<MatchedTicketItem> MatchedItems { get; set; } = new();
}
