using System.Threading.Tasks;

namespace OneCardExpenseValidator.API.Services
{
    public interface IClaudeService
    {
        Task<string> GenerateResponseAsync(string prompt);
    }
}
