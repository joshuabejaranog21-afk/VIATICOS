using System;

namespace OneCardExpenseValidator.Application.DTOs
{
    /// <summary>
    /// DTO para la sesión de validación
    /// Contiene la información necesaria para establecer la conexión QR entre PC y móvil
    /// </summary>
    public class ValidationSessionDto
    {
        /// <summary>
        /// ID único de la sesión (GUID)
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Código QR en formato Base64 (imagen PNG)
        /// </summary>
        public string QrCodeBase64 { get; set; } = string.Empty;

        /// <summary>
        /// URL completa para escanear con el móvil
        /// </summary>
        public string MobileUrl { get; set; } = string.Empty;

        /// <summary>
        /// Estado de la sesión: "Created", "Connected", "Processing", "Completed", "Expired"
        /// </summary>
        public string Status { get; set; } = "Created";

        /// <summary>
        /// Fecha y hora de creación de la sesión
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora de expiración (10 minutos después de creación)
        /// </summary>
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

        /// <summary>
        /// ID de conexión de SignalR del dispositivo PC (admin)
        /// </summary>
        public string? AdminConnectionId { get; set; }

        /// <summary>
        /// ID de conexión de SignalR del dispositivo móvil
        /// </summary>
        public string? MobileConnectionId { get; set; }
    }

    /// <summary>
    /// DTO para la solicitud de validación de producto
    /// Enviado desde el móvil al servidor
    /// </summary>
    public class ValidationRequestDto
    {
        /// <summary>
        /// ID de la sesión activa
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Imagen del producto en formato Base64
        /// </summary>
        public string ImageBase64 { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional proporcionada por el usuario
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Marca de tiempo del lado del cliente
        /// </summary>
        public DateTime ClientTimestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO para la respuesta de validación
    /// Enviado desde el servidor a PC y móvil después del análisis
    /// </summary>
    public class ValidationResponseDto
    {
        /// <summary>
        /// ID único de este resultado de validación
        /// </summary>
        public string ValidationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nombre del producto identificado
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Categoría del producto (ej: "Alimentos", "Tecnología", "Transporte")
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el producto es deducible según políticas empresariales
        /// </summary>
        public bool IsDeductible { get; set; }

        /// <summary>
        /// Nivel de confianza del análisis (0.0 a 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Razón detallada de por qué es o no deducible
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Marca de tiempo del análisis
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Método utilizado para la categorización: "Claude", "Keywords", "Manual"
        /// </summary>
        public string AnalysisMethod { get; set; } = string.Empty;

        /// <summary>
        /// Imagen original en miniatura (opcional, para historial)
        /// </summary>
        public string? ThumbnailBase64 { get; set; }

        /// <summary>
        /// Indica si requiere revisión manual adicional
        /// </summary>
        public bool RequiresManualReview { get; set; }

        /// <summary>
        /// Notas adicionales del sistema
        /// </summary>
        public string? AdditionalNotes { get; set; }
    }

    /// <summary>
    /// DTO para el estado de la sesión
    /// Se envía periódicamente para mantener sincronización entre dispositivos
    /// </summary>
    public class SessionStatusDto
    {
        /// <summary>
        /// ID de la sesión
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Indica si hay un dispositivo móvil conectado
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Tipo de dispositivo conectado: "Admin", "Mobile", "None"
        /// </summary>
        public string DeviceType { get; set; } = "None";

        /// <summary>
        /// Última actividad registrada
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Número de validaciones procesadas en esta sesión
        /// </summary>
        public int ProcessedCount { get; set; }

        /// <summary>
        /// Estado actual: "Waiting", "Ready", "Processing", "Error"
        /// </summary>
        public string CurrentStatus { get; set; } = "Waiting";

        /// <summary>
        /// Mensaje de estado para mostrar al usuario
        /// </summary>
        public string StatusMessage { get; set; } = "Esperando conexión...";
    }

    /// <summary>
    /// DTO para respuesta de Claude Vision API
    /// Mapea la estructura JSON que devuelve Claude
    /// </summary>
    public class ClaudeVisionResponseDto
    {
        /// <summary>
        /// Nombre del producto identificado
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Categoría del producto
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Es deducible
        /// </summary>
        public bool IsDeductible { get; set; }

        /// <summary>
        /// Nivel de confianza (0.0 a 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Razón detallada
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Subcategoría opcional
        /// </summary>
        public string? Subcategory { get; set; }

        /// <summary>
        /// Precio estimado (si es visible en la imagen)
        /// </summary>
        public decimal? EstimatedPrice { get; set; }
    }

    /// <summary>
    /// DTO para errores en la validación
    /// </summary>
    public class ValidationErrorDto
    {
        /// <summary>
        /// Código de error
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje de error
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// ID de la sesión (si aplica)
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Marca de tiempo del error
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si el error es recuperable
        /// </summary>
        public bool IsRecoverable { get; set; }

        /// <summary>
        /// Acción sugerida para el usuario
        /// </summary>
        public string? SuggestedAction { get; set; }
    }

    /// <summary>
    /// DTO para el historial de validaciones en una sesión
    /// </summary>
    public class ValidationHistoryItemDto
    {
        public string ValidationId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public bool IsDeductible { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ThumbnailBase64 { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para el análisis completo de un recibo
    /// </summary>
    public class TicketAnalysisResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Vendor { get; set; }
        public string? TicketDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<TicketProductDto>? Products { get; set; }
    }

    /// <summary>
    /// DTO para un producto extraído del recibo
    /// </summary>
    public class TicketProductDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsDeductible { get; set; }
        public string Reason { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public bool ManuallyOverridden { get; set; } = false;
    }

    /// <summary>
    /// DTO para la categorización de un producto individual por nombre
    /// </summary>
    public class ProductCategorizationDto
    {
        public bool Success { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsDeductible { get; set; }
        public string Reason { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar manualmente el estado de deducibilidad de un producto
    /// </summary>
    public class UpdateProductDeductibilityDto
    {
        public string ProductId { get; set; } = string.Empty;
        public bool IsDeductible { get; set; }
        public string? Reason { get; set; }
    }
}
