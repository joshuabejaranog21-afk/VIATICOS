using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QRCoder;
using OneCardExpenseValidator.Application.DTOs;
using OneCardExpenseValidator.Application.Hubs;
using OneCardExpenseValidator.Application.Services;

namespace OneCardExpenseValidator.API.Controllers.API
{
    /// <summary>
    /// Controlador API para validación de productos en tiempo real
    /// Maneja creación de sesiones QR, análisis de imágenes y comunicación vía SignalR
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ValidationController : ControllerBase
    {
        private readonly ICategorizationService _categorizationService;
        private readonly IHubContext<ValidationHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ValidationController> _logger;

        public ValidationController(
            ICategorizationService categorizationService,
            IHubContext<ValidationHub> hubContext,
            IConfiguration configuration,
            ILogger<ValidationController> logger)
        {
            _categorizationService = categorizationService;
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/validation/session/create
        /// Crea una nueva sesión de validación y genera código QR
        /// </summary>
        /// <returns>Sesión con QR code en Base64</returns>
        [HttpPost("session/create")]
        public IActionResult CreateSession()
        {
            try
            {
                // Generar ID único para la sesión
                var sessionId = Guid.NewGuid().ToString("N");

                _logger.LogInformation("Creando nueva sesión de validación: {SessionId}", sessionId);

                // Obtener la URL base de la aplicación
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var mobileUrl = $"{baseUrl}/Validation/Mobile?session={sessionId}";

                // Generar código QR
                var qrCodeBase64 = GenerateQRCode(mobileUrl);

                var session = new ValidationSessionDto
                {
                    SessionId = sessionId,
                    QrCodeBase64 = qrCodeBase64,
                    MobileUrl = mobileUrl,
                    Status = "Created",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10)
                };

                _logger.LogInformation("Sesión {SessionId} creada exitosamente", sessionId);

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sesión de validación");
                return StatusCode(500, new ValidationErrorDto
                {
                    ErrorCode = "CREATE_SESSION_ERROR",
                    Message = "Error al crear la sesión de validación",
                    Timestamp = DateTime.UtcNow,
                    IsRecoverable = true,
                    SuggestedAction = "Por favor, intente nuevamente"
                });
            }
        }

        /// <summary>
        /// GET /api/validation/session/{sessionId}
        /// Obtiene información de una sesión existente
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        [HttpGet("session/{sessionId}")]
        public IActionResult GetSession(string sessionId)
        {
            try
            {
                _logger.LogInformation("Consultando sesión: {SessionId}", sessionId);

                // En una implementación real, buscarías la sesión en BD o cache
                // Por ahora retornamos info básica
                return Ok(new
                {
                    sessionId,
                    exists = true,
                    message = "Sesión encontrada"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión {SessionId}", sessionId);
                return NotFound(new ValidationErrorDto
                {
                    ErrorCode = "SESSION_NOT_FOUND",
                    Message = "Sesión no encontrada",
                    SessionId = sessionId,
                    Timestamp = DateTime.UtcNow,
                    IsRecoverable = false,
                    SuggestedAction = "Escanee un nuevo código QR"
                });
            }
        }

        /// <summary>
        /// POST /api/validation/analyze
        /// Analiza una imagen de producto y retorna el resultado vía SignalR
        /// </summary>
        /// <param name="request">Request con imagen y datos de sesión</param>
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeProduct([FromBody] ValidationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Analizando producto para sesión {SessionId}", request.SessionId);

                // Validar que la imagen no esté vacía
                if (string.IsNullOrWhiteSpace(request.ImageBase64))
                {
                    return BadRequest(new ValidationErrorDto
                    {
                        ErrorCode = "INVALID_IMAGE",
                        Message = "La imagen es requerida",
                        SessionId = request.SessionId,
                        Timestamp = DateTime.UtcNow,
                        IsRecoverable = true,
                        SuggestedAction = "Por favor, tome una foto del producto"
                    });
                }

                // Notificar vía SignalR que estamos procesando
                await NotifyProcessingStart(request.SessionId);

                // Analizar el producto usando el servicio de categorización
                var result = await _categorizationService.AnalyzeProductImageAsync(
                    request.ImageBase64,
                    request.Description
                );

                // Generar thumbnail de la imagen para historial
                result.ThumbnailBase64 = GenerateThumbnail(request.ImageBase64);

                // Enviar resultado vía SignalR a ambos dispositivos
                await _hubContext.Clients.Group(request.SessionId).SendAsync("ValidationResult", result);

                // También retornar vía HTTP para el cliente que hizo el request
                _logger.LogInformation(
                    "Análisis completado para sesión {SessionId}: {ProductName} - {IsDeductible}",
                    request.SessionId,
                    result.ProductName,
                    result.IsDeductible ? "DEDUCIBLE" : "NO DEDUCIBLE"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar producto para sesión {SessionId}", request.SessionId);

                var error = new ValidationErrorDto
                {
                    ErrorCode = "ANALYSIS_ERROR",
                    Message = $"Error al analizar el producto: {ex.Message}",
                    SessionId = request.SessionId,
                    Timestamp = DateTime.UtcNow,
                    IsRecoverable = true,
                    SuggestedAction = "Por favor, intente nuevamente con otra imagen"
                };

                // Notificar error vía SignalR
                await _hubContext.Clients.Group(request.SessionId).SendAsync("ValidationError", error);

                return StatusCode(500, error);
            }
        }

        /// <summary>
        /// POST /api/validation/test-claude
        /// Endpoint de prueba para verificar conexión con Claude API
        /// </summary>
        [HttpPost("test-claude")]
        public async Task<IActionResult> TestClaudeConnection([FromBody] TestClaudeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Probando conexión con Claude API");

                var result = await _categorizationService.AnalyzeProductImageAsync(
                    request.ImageBase64,
                    "Test de conexión"
                );

                return Ok(new
                {
                    success = true,
                    message = "Conexión con Claude API exitosa",
                    result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar Claude API");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al conectar con Claude API",
                    error = ex.Message
                });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Genera código QR a partir de una URL
        /// </summary>
        /// <param name="url">URL a codificar en el QR</param>
        /// <returns>Imagen QR en Base64</returns>
        private string GenerateQRCode(string url)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20); // 20 pixels por módulo

                return Convert.ToBase64String(qrCodeImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar código QR");
                throw;
            }
        }

        /// <summary>
        /// Genera thumbnail de una imagen Base64
        /// </summary>
        /// <param name="imageBase64">Imagen original en Base64</param>
        /// <returns>Thumbnail en Base64 (más pequeño)</returns>
        private string? GenerateThumbnail(string imageBase64)
        {
            try
            {
                // Por ahora, simplemente truncamos la imagen
                // En una implementación real, usarías una librería de procesamiento de imágenes
                // como ImageSharp o System.Drawing para redimensionar

                // Limpiar el prefijo si existe
                var cleanBase64 = imageBase64.Contains(",")
                    ? imageBase64.Split(',')[1]
                    : imageBase64;

                // Truncar a los primeros ~50KB (aproximadamente)
                if (cleanBase64.Length > 70000)
                {
                    return cleanBase64.Substring(0, 70000);
                }

                return cleanBase64;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al generar thumbnail, usando imagen original");
                return null;
            }
        }

        /// <summary>
        /// Notifica vía SignalR que el procesamiento ha iniciado
        /// </summary>
        private async Task NotifyProcessingStart(string sessionId)
        {
            try
            {
                await _hubContext.Clients.Group(sessionId).SendAsync("ProcessingStarted", new
                {
                    sessionId,
                    message = "Analizando producto...",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al notificar inicio de procesamiento");
            }
        }

        #endregion

        #region DTOs

        /// <summary>
        /// DTO para prueba de Claude
        /// </summary>
        public class TestClaudeRequestDto
        {
            public string ImageBase64 { get; set; } = string.Empty;
        }

        #endregion
    }
}
