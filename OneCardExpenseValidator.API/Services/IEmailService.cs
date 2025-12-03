namespace OneCardExpenseValidator.API.Services
{
    public interface IEmailService
    {
        Task SendSpendingLimitWarningAsync(string employeeEmail, string employeeName, decimal currentSpending, decimal limit, string period, int percentage);
        Task SendSpendingLimitReachedAsync(string employeeEmail, string employeeName, decimal currentSpending, decimal limit, string period);
    }
}
