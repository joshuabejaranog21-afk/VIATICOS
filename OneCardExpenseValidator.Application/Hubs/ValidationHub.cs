using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OneCardExpenseValidator.Application.DTOs;

namespace OneCardExpenseValidator.Application.Hubs
{
    /// <summary>
    /// Hub de SignalR para comunicación en tiempo real entre PC (admin) y móvil
    /// Maneja sesiones de validación, envío de imágenes y resultados
    /// </summary>
    public class ValidationHub : Hub
    {
        private readonly ILogger<ValidationHub> _logger;

        // Diccionario estático para mantener sesiones activas en memoria
        // En producción, considerar usar Redis o similar para escalabilidad
        private static readonly ConcurrentDictionary<string, ValidationSession> ActiveSessions = new();

        // Mapeo de ConnectionId a SessionId para cleanup
        private static readonly ConcurrentDictionary<string, string> ConnectionToSession = new();

        public ValidationHub(ILogger<ValidationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva sesión de validación (llamado desde PC/Admin)
        /// </summary>
        /// <param name="sessionId">ID único de la sesión (generado por el controlador)</param>
        public async Task CreateSession(string sessionId)
        {
            try
            {
                var connectionId = Context.ConnectionId;

                _logger.LogInformation("Creando sesión {SessionId} para conexión {ConnectionId}", sessionId, connectionId);

                var session = new ValidationSession
                {
                    SessionId = sessionId,
                    AdminConnectionId = connectionId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    Status = "Created"
                };

                if (ActiveSessions.TryAdd(sessionId, session))
                {
                    ConnectionToSession[connectionId] = sessionId;

                    await Clients.Caller.SendAsync("SessionCreated", new
                    {
                        sessionId,
                        status = "Created",
                        message = "Sesión creada exitosamente. Esperando conexión del móvil..."
                    });

                    _logger.LogInformation("Sesión {SessionId} creada exitosamente", sessionId);
                }
                else
                {
                    _logger.LogWarning("Sesión {SessionId} ya existe", sessionId);
                    await Clients.Caller.SendAsync("Error", new
                    {
                        code = "SESSION_EXISTS",
                        message = "La sesión ya existe"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sesión {SessionId}", sessionId);
                await Clients.Caller.SendAsync("Error", new
                {
                    code = "CREATE_SESSION_ERROR",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Móvil se une a una sesión existente (después de escanear QR)
        /// </summary>
        /// <param name="sessionId">ID de la sesión del código QR</param>
        public async Task JoinSession(string sessionId)
        {
            try
            {
                var connectionId = Context.ConnectionId;

                _logger.LogInformation("Móvil {ConnectionId} intentando unirse a sesión {SessionId}", connectionId, sessionId);

                if (ActiveSessions.TryGetValue(sessionId, out var session))
                {
                    // Verificar si la sesión no ha expirado
                    if (DateTime.UtcNow > session.ExpiresAt)
                    {
                        _logger.LogWarning("Sesión {SessionId} ha expirado", sessionId);
                        await Clients.Caller.SendAsync("Error", new
                        {
                            code = "SESSION_EXPIRED",
                            message = "La sesión ha expirado. Por favor, escanee un nuevo código QR."
                        });
                        return;
                    }

                    // Registrar conexión del móvil
                    session.MobileConnectionId = connectionId;
                    session.Status = "Connected";
                    session.LastActivity = DateTime.UtcNow;

                    ConnectionToSession[connectionId] = sessionId;

                    // Notificar al móvil que está conectado
                    await Clients.Caller.SendAsync("JoinedSession", new
                    {
                        sessionId,
                        status = "Connected",
                        message = "Conectado exitosamente. Puede comenzar a tomar fotos."
                    });

                    // Notificar al admin (PC) que el móvil se conectó
                    if (!string.IsNullOrEmpty(session.AdminConnectionId))
                    {
                        await Clients.Client(session.AdminConnectionId).SendAsync("MobileConnected", new
                        {
                            sessionId,
                            connectedAt = DateTime.UtcNow,
                            message = "Teléfono móvil conectado ✓"
                        });
                    }

                    _logger.LogInformation("Móvil conectado exitosamente a sesión {SessionId}", sessionId);
                }
                else
                {
                    _logger.LogWarning("Sesión {SessionId} no encontrada", sessionId);
                    await Clients.Caller.SendAsync("Error", new
                    {
                        code = "SESSION_NOT_FOUND",
                        message = "Sesión no encontrada. Verifique el código QR."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unirse a sesión {SessionId}", sessionId);
                await Clients.Caller.SendAsync("Error", new
                {
                    code = "JOIN_SESSION_ERROR",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Móvil envía una imagen para validación
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        /// <param name="imageBase64">Imagen en Base64</param>
        /// <param name="description">Descripción opcional del producto</param>
        public async Task SendImage(string sessionId, string imageBase64, string? description = null)
        {
            try
            {
                var connectionId = Context.ConnectionId;

                _logger.LogInformation("Imagen recibida de móvil {ConnectionId} para sesión {SessionId}", connectionId, sessionId);

                if (ActiveSessions.TryGetValue(sessionId, out var session))
                {
                    session.Status = "Processing";
                    session.LastActivity = DateTime.UtcNow;
                    session.ProcessedCount++;

                    // Notificar a ambos dispositivos que se está procesando
                    var processingNotification = new
                    {
                        sessionId,
                        status = "Processing",
                        message = "Procesando imagen...",
                        imagePreview = TruncateBase64ForPreview(imageBase64)
                    };

                    // Enviar al admin (PC)
                    if (!string.IsNullOrEmpty(session.AdminConnectionId))
                    {
                        await Clients.Client(session.AdminConnectionId).SendAsync("ImageReceived", new
                        {
                            sessionId,
                            imageBase64, // Admin recibe imagen completa
                            description,
                            receivedAt = DateTime.UtcNow,
                            message = "Imagen recibida, analizando..."
                        });
                    }

                    // Enviar confirmación al móvil
                    await Clients.Caller.SendAsync("ImageSent", processingNotification);

                    _logger.LogInformation("Imagen enviada a admin para procesamiento");
                }
                else
                {
                    _logger.LogWarning("Sesión {SessionId} no encontrada al enviar imagen", sessionId);
                    await Clients.Caller.SendAsync("Error", new
                    {
                        code = "SESSION_NOT_FOUND",
                        message = "Sesión no encontrada"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar imagen para sesión {SessionId}", sessionId);
                await Clients.Caller.SendAsync("Error", new
                {
                    code = "SEND_IMAGE_ERROR",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Servidor envía el resultado del análisis a ambos dispositivos
        /// (Llamado desde ValidationController después de procesar con Claude)
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        /// <param name="result">Resultado del análisis</param>
        public async Task SendResult(string sessionId, ValidationResponseDto result)
        {
            try
            {
                _logger.LogInformation("Enviando resultado para sesión {SessionId}: {ProductName}", sessionId, result.ProductName);

                if (ActiveSessions.TryGetValue(sessionId, out var session))
                {
                    session.Status = "Completed";
                    session.LastActivity = DateTime.UtcNow;

                    // Agregar al historial de la sesión
                    session.History.Add(new ValidationHistoryItemDto
                    {
                        ValidationId = result.ValidationId,
                        ProductName = result.ProductName,
                        IsDeductible = result.IsDeductible,
                        Category = result.Category,
                        Timestamp = result.Timestamp,
                        ThumbnailBase64 = result.ThumbnailBase64 ?? ""
                    });

                    // Enviar resultado completo al admin (PC)
                    if (!string.IsNullOrEmpty(session.AdminConnectionId))
                    {
                        await Clients.Client(session.AdminConnectionId).SendAsync("ValidationResult", result);
                    }

                    // Enviar resultado simplificado al móvil
                    await Clients.Client(session.MobileConnectionId!).SendAsync("ValidationResult", new
                    {
                        result.ValidationId,
                        result.ProductName,
                        result.IsDeductible,
                        result.Category,
                        result.Reason,
                        result.Confidence,
                        message = result.IsDeductible
                            ? "✅ DEDUCIBLE"
                            : "❌ NO DEDUCIBLE"
                    });

                    // Cambiar estado de vuelta a "Ready" para siguiente validación
                    session.Status = "Ready";

                    _logger.LogInformation("Resultado enviado exitosamente para sesión {SessionId}", sessionId);
                }
                else
                {
                    _logger.LogWarning("Sesión {SessionId} no encontrada al enviar resultado", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar resultado para sesión {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Obtiene el estado actual de una sesión
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        public async Task GetSessionStatus(string sessionId)
        {
            try
            {
                if (ActiveSessions.TryGetValue(sessionId, out var session))
                {
                    var status = new SessionStatusDto
                    {
                        SessionId = sessionId,
                        IsConnected = !string.IsNullOrEmpty(session.MobileConnectionId),
                        DeviceType = Context.ConnectionId == session.AdminConnectionId ? "Admin" : "Mobile",
                        LastActivity = session.LastActivity,
                        ProcessedCount = session.ProcessedCount,
                        CurrentStatus = session.Status,
                        StatusMessage = GetStatusMessage(session.Status, session.MobileConnectionId != null)
                    };

                    await Clients.Caller.SendAsync("SessionStatus", status);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", new
                    {
                        code = "SESSION_NOT_FOUND",
                        message = "Sesión no encontrada"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de sesión {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Obtiene el historial de validaciones de una sesión
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        public async Task GetSessionHistory(string sessionId)
        {
            try
            {
                if (ActiveSessions.TryGetValue(sessionId, out var session))
                {
                    await Clients.Caller.SendAsync("SessionHistory", session.History);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de sesión {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Cierra una sesión manualmente
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        public async Task CloseSession(string sessionId)
        {
            try
            {
                _logger.LogInformation("Cerrando sesión {SessionId}", sessionId);

                if (ActiveSessions.TryRemove(sessionId, out var session))
                {
                    // Notificar a ambos dispositivos
                    if (!string.IsNullOrEmpty(session.AdminConnectionId))
                    {
                        await Clients.Client(session.AdminConnectionId).SendAsync("SessionClosed", new
                        {
                            sessionId,
                            message = "Sesión cerrada"
                        });
                    }

                    if (!string.IsNullOrEmpty(session.MobileConnectionId))
                    {
                        await Clients.Client(session.MobileConnectionId).SendAsync("SessionClosed", new
                        {
                            sessionId,
                            message = "Sesión cerrada"
                        });
                    }

                    // Limpiar mapeos
                    if (!string.IsNullOrEmpty(session.AdminConnectionId))
                    {
                        ConnectionToSession.TryRemove(session.AdminConnectionId, out _);
                    }
                    if (!string.IsNullOrEmpty(session.MobileConnectionId))
                    {
                        ConnectionToSession.TryRemove(session.MobileConnectionId, out _);
                    }

                    _logger.LogInformation("Sesión {SessionId} cerrada exitosamente", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Override para limpiar cuando una conexión se desconecta
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            _logger.LogInformation("Conexión {ConnectionId} desconectada", connectionId);

            // Buscar la sesión asociada a esta conexión
            if (ConnectionToSession.TryRemove(connectionId, out var sessionId))
            {
                if (ActiveSessions.TryGetValue(sessionId, out var session))
                {
                    // Notificar al otro dispositivo
                    if (session.AdminConnectionId == connectionId)
                    {
                        // Admin se desconectó
                        if (!string.IsNullOrEmpty(session.MobileConnectionId))
                        {
                            await Clients.Client(session.MobileConnectionId).SendAsync("AdminDisconnected", new
                            {
                                sessionId,
                                message = "El administrador se ha desconectado"
                            });
                        }
                    }
                    else if (session.MobileConnectionId == connectionId)
                    {
                        // Móvil se desconectó
                        if (!string.IsNullOrEmpty(session.AdminConnectionId))
                        {
                            await Clients.Client(session.AdminConnectionId).SendAsync("MobileDisconnected", new
                            {
                                sessionId,
                                message = "El dispositivo móvil se ha desconectado"
                            });
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        #region Helper Methods

        /// <summary>
        /// Trunca el Base64 para preview (primeros 100 caracteres)
        /// </summary>
        private string TruncateBase64ForPreview(string base64)
        {
            return base64.Length > 100 ? base64.Substring(0, 100) + "..." : base64;
        }

        /// <summary>
        /// Genera mensaje de estado basado en el estado actual
        /// </summary>
        private string GetStatusMessage(string status, bool mobileConnected)
        {
            return status switch
            {
                "Created" => "Esperando conexión del móvil...",
                "Connected" => "Móvil conectado. Listo para validar.",
                "Processing" => "Procesando imagen...",
                "Completed" => "Validación completada",
                "Ready" => "Listo para siguiente validación",
                "Error" => "Error en la validación",
                _ => "Estado desconocido"
            };
        }

        #endregion

        #region Session Model

        /// <summary>
        /// Modelo interno para representar una sesión activa
        /// </summary>
        private class ValidationSession
        {
            public string SessionId { get; set; } = string.Empty;
            public string? AdminConnectionId { get; set; }
            public string? MobileConnectionId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
            public DateTime LastActivity { get; set; }
            public string Status { get; set; } = "Created";
            public int ProcessedCount { get; set; }
            public List<ValidationHistoryItemDto> History { get; set; } = new();
        }

        #endregion

        /// <summary>
        /// Método estático para limpiar sesiones expiradas (puede ser llamado por un background service)
        /// </summary>
        public static void CleanupExpiredSessions()
        {
            var now = DateTime.UtcNow;
            var expiredSessions = ActiveSessions
                .Where(kvp => kvp.Value.ExpiresAt < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionId in expiredSessions)
            {
                ActiveSessions.TryRemove(sessionId, out _);
            }
        }
    }
}
