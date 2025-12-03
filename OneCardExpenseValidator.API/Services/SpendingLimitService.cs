using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Services
{
    public class SpendingLimitService : ISpendingLimitService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SpendingLimitService> _logger;

        public SpendingLimitService(
            AppDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<SpendingLimitService> logger)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<decimal> GetDailySpendingAsync(int employeeId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var dailyTotal = await _context.ExpenseTickets
                .Where(t => t.EmployeeId == employeeId &&
                           t.TicketDate == today &&
                           (t.ValidationStatus == "Approved" || t.ValidationStatus == "Pending"))
                .SumAsync(t => t.TotalAmount);

            return dailyTotal;
        }

        public async Task<decimal> GetMonthlySpendingAsync(int employeeId)
        {
            var now = DateTime.Now;
            var startOfMonth = DateOnly.FromDateTime(new DateTime(now.Year, now.Month, 1));
            var startOfNextMonth = DateOnly.FromDateTime(new DateTime(now.Year, now.Month, 1).AddMonths(1));

            var monthlyTotal = await _context.ExpenseTickets
                .Where(t => t.EmployeeId == employeeId &&
                           t.TicketDate >= startOfMonth &&
                           t.TicketDate < startOfNextMonth &&
                           (t.ValidationStatus == "Approved" || t.ValidationStatus == "Pending"))
                .SumAsync(t => t.TotalAmount);

            return monthlyTotal;
        }

        public async Task<bool> WillExceedDailyLimitAsync(int employeeId, decimal newAmount)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            var currentSpending = await GetDailySpendingAsync(employeeId);
            return (currentSpending + newAmount) > employee.DailyExpenseLimit;
        }

        public async Task<bool> WillExceedMonthlyLimitAsync(int employeeId, decimal newAmount)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            var currentSpending = await GetMonthlySpendingAsync(employeeId);
            return (currentSpending + newAmount) > employee.MonthlyExpenseLimit;
        }

        public async Task CheckAndNotifySpendingLimitAsync(int employeeId)
        {
            _logger.LogInformation($"🔔 CheckAndNotifySpendingLimitAsync llamado para EmployeeId: {employeeId}");

            var enableNotifications = _configuration.GetValue<bool>("SpendingLimitNotifications:EnableNotifications", true);
            _logger.LogInformation($"Notificaciones habilitadas: {enableNotifications}");

            if (!enableNotifications)
            {
                _logger.LogWarning("Notificaciones deshabilitadas en configuración");
                return;
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                _logger.LogWarning($"❌ Employee {employeeId} no encontrado");
                return;
            }

            if (string.IsNullOrEmpty(employee.Email))
            {
                _logger.LogWarning($"❌ Employee {employeeId} no tiene email configurado");
                return;
            }

            _logger.LogInformation($"✅ Employee encontrado: {employee.FirstName} {employee.LastName} ({employee.Email})");

            var employeeName = $"{employee.FirstName} {employee.LastName}";
            var warningPercentages = _configuration.GetSection("SpendingLimitNotifications:WarningPercentages")
                .Get<int[]>() ?? new[] { 80, 90 };

            _logger.LogInformation($"Porcentajes de advertencia: {string.Join(", ", warningPercentages)}%");

            // Check daily limit
            var dailySpending = await GetDailySpendingAsync(employeeId);
            var dailyLimit = employee.DailyExpenseLimit ?? 0m;
            var dailyPercentage = dailyLimit > 0 ? (int)((dailySpending / dailyLimit) * 100) : 0;

            _logger.LogInformation($"💰 Gasto DIARIO: ${dailySpending:N2} / ${dailyLimit:N2} = {dailyPercentage}%");

            await SendNotificationIfNeededAsync(
                employee.Email,
                employeeName,
                dailySpending,
                dailyLimit,
                dailyPercentage,
                "diario",
                warningPercentages);

            // Check monthly limit
            var monthlySpending = await GetMonthlySpendingAsync(employeeId);
            var monthlyLimit = employee.MonthlyExpenseLimit ?? 0m;
            var monthlyPercentage = monthlyLimit > 0 ? (int)((monthlySpending / monthlyLimit) * 100) : 0;

            await SendNotificationIfNeededAsync(
                employee.Email,
                employeeName,
                monthlySpending,
                monthlyLimit,
                monthlyPercentage,
                "mensual",
                warningPercentages);
        }

        private async Task SendNotificationIfNeededAsync(
            string employeeEmail,
            string employeeName,
            decimal currentSpending,
            decimal limit,
            int percentage,
            string period,
            int[] warningPercentages)
        {
            try
            {
                _logger.LogInformation($"📧 SendNotificationIfNeededAsync - Periodo: {period}, Porcentaje: {percentage}%");

                var sendOnLimitReached = _configuration.GetValue<bool>("SpendingLimitNotifications:SendOnLimitReached", true);

                // Si alcanzó o superó el 100%
                if (percentage >= 100 && sendOnLimitReached)
                {
                    _logger.LogInformation($"🚨 Límite {period} ALCANZADO ({percentage}%) - Enviando notificación...");

                    await _emailService.SendSpendingLimitReachedAsync(
                        employeeEmail,
                        employeeName,
                        currentSpending,
                        limit,
                        period);

                    _logger.LogInformation($"✅ Notificación de límite alcanzado enviada a {employeeEmail}");
                }
                // Si alcanzó alguno de los porcentajes de advertencia (80%, 90%, etc)
                else if (warningPercentages.Any(wp => percentage >= wp && percentage < 100))
                {
                    var highestWarning = warningPercentages.Where(wp => percentage >= wp).Max();

                    _logger.LogInformation($"⚠️ Límite {period} en {highestWarning}% ({percentage}%) - Enviando advertencia...");

                    await _emailService.SendSpendingLimitWarningAsync(
                        employeeEmail,
                        employeeName,
                        currentSpending,
                        limit,
                        period,
                        highestWarning);

                    _logger.LogInformation($"✅ Advertencia de {highestWarning}% enviada a {employeeEmail}");
                }
                else
                {
                    _logger.LogInformation($"ℹ️ No se requiere notificación para {period} ({percentage}%)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to {employeeEmail}");
            }
        }
    }
}
