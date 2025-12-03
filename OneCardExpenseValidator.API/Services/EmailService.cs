using System.Net;
using System.Net.Mail;

namespace OneCardExpenseValidator.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendSpendingLimitWarningAsync(string employeeEmail, string employeeName, decimal currentSpending, decimal limit, string period, int percentage)
        {
            var subject = $"⚠️ Alerta: Has usado el {percentage}% de tu límite de gastos {period}";

            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #ff9800;'>Alerta de Límite de Gastos</h2>
                    <p>Hola <strong>{employeeName}</strong>,</p>
                    <p>Este es un recordatorio de que has alcanzado el <strong>{percentage}%</strong> de tu límite de gastos {period}.</p>

                    <div style='background-color: #fff3e0; padding: 15px; border-left: 4px solid #ff9800; margin: 20px 0;'>
                        <p style='margin: 5px 0;'><strong>Gasto actual:</strong> ${currentSpending:N2}</p>
                        <p style='margin: 5px 0;'><strong>Límite {period}:</strong> ${limit:N2}</p>
                        <p style='margin: 5px 0;'><strong>Disponible:</strong> ${(limit - currentSpending):N2}</p>
                    </div>

                    <p>Por favor, gestiona tus gastos cuidadosamente para no exceder tu límite.</p>
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        Este es un mensaje automático del sistema OneCard Expense Validator.
                    </p>
                </body>
                </html>";

            await SendEmailAsync(employeeEmail, subject, body);
        }

        public async Task SendSpendingLimitReachedAsync(string employeeEmail, string employeeName, decimal currentSpending, decimal limit, string period)
        {
            var subject = $"🚨 IMPORTANTE: Has alcanzado tu límite de gastos {period}";

            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #f44336;'>Límite de Gastos Alcanzado</h2>
                    <p>Hola <strong>{employeeName}</strong>,</p>
                    <p>Has alcanzado o superado tu límite de gastos {period}.</p>

                    <div style='background-color: #ffebee; padding: 15px; border-left: 4px solid #f44336; margin: 20px 0;'>
                        <p style='margin: 5px 0;'><strong>Gasto actual:</strong> ${currentSpending:N2}</p>
                        <p style='margin: 5px 0;'><strong>Límite {period}:</strong> ${limit:N2}</p>
                        <p style='margin: 5px 0; color: #f44336;'><strong>Excedente:</strong> ${(currentSpending - limit):N2}</p>
                    </div>

                    <p><strong>No podrás realizar más gastos hasta que tu límite sea ajustado o se renueve el período.</strong></p>
                    <p>Por favor, contacta a tu supervisor si necesitas aumentar tu límite.</p>

                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        Este es un mensaje automático del sistema OneCard Expense Validator.
                    </p>
                </body>
                </html>";

            await SendEmailAsync(employeeEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                _logger.LogInformation($"==================== INICIANDO ENVÍO DE EMAIL ====================");
                _logger.LogInformation($"Destinatario: {toEmail}");
                _logger.LogInformation($"Asunto: {subject}");

                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "OneCard Expense Validator";
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                _logger.LogInformation($"SMTP Host: {smtpHost}");
                _logger.LogInformation($"SMTP Port: {smtpPort}");
                _logger.LogInformation($"SMTP Username: {smtpUsername}");
                _logger.LogInformation($"SMTP Password: {(string.IsNullOrEmpty(smtpPassword) ? "NO CONFIGURADA" : "********")}");
                _logger.LogInformation($"From Email: {fromEmail}");
                _logger.LogInformation($"Enable SSL: {enableSsl}");

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername))
                {
                    _logger.LogWarning("❌ Email configuration is missing. Email not sent.");
                    return;
                }

                if (string.IsNullOrEmpty(smtpPassword) || smtpPassword == "tu-contraseña-de-aplicacion")
                {
                    _logger.LogWarning("❌ SMTP Password not configured properly. Email not sent.");
                    return;
                }

                _logger.LogInformation("Creando cliente SMTP...");
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = enableSsl
                };

                _logger.LogInformation("Creando mensaje de email...");
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUsername, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                _logger.LogInformation("Enviando email...");
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"✅ Email sent successfully to {toEmail}");
                _logger.LogInformation($"==================================================================");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"❌ SMTP ERROR: {smtpEx.StatusCode} - {smtpEx.Message}");
                _logger.LogError($"Stack: {smtpEx.StackTrace}");
                if (smtpEx.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {smtpEx.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ GENERAL ERROR sending email to {toEmail}");
                _logger.LogError($"Error: {ex.Message}");
                _logger.LogError($"Stack: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
