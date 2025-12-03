using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Services
{
    public interface ISpendingLimitService
    {
        Task CheckAndNotifySpendingLimitAsync(int employeeId);
        Task<decimal> GetDailySpendingAsync(int employeeId);
        Task<decimal> GetMonthlySpendingAsync(int employeeId);
        Task<bool> WillExceedDailyLimitAsync(int employeeId, decimal newAmount);
        Task<bool> WillExceedMonthlyLimitAsync(int employeeId, decimal newAmount);
    }
}
