using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OneCardExpenseValidator.API.Controllers
{
    /// <summary>
    /// Controlador MVC para las vistas de validación
    /// Maneja las páginas web de Admin (PC) y Mobile (captura)
    /// </summary>
    public class ValidationController : Controller
    {
        private readonly ILogger<ValidationController> _logger;

        public ValidationController(ILogger<ValidationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// GET /Validation/Admin
        /// Vista de administración para PC - muestra QR y recibe resultados
        /// </summary>
        [HttpGet]
        public IActionResult Admin()
        {
            _logger.LogInformation("Accediendo a vista de Admin Validation");
            return View();
        }

        /// <summary>
        /// GET /Validation/Mobile?session={sessionId}
        /// Vista móvil para captura de fotos - se abre después de escanear QR
        /// </summary>
        /// <param name="session">ID de sesión del código QR</param>
        [HttpGet]
        public IActionResult Mobile(string? session)
        {
            _logger.LogInformation("Accediendo a vista Mobile con sesión: {SessionId}", session ?? "Sin sesión");

            if (string.IsNullOrWhiteSpace(session))
            {
                ViewBag.Error = "No se proporcionó ID de sesión. Por favor, escanee el código QR nuevamente.";
            }

            ViewBag.SessionId = session;
            return View();
        }
    }
}
