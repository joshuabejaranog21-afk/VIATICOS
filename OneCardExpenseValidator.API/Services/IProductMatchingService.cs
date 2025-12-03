using OneCardExpenseValidator.API.Models;

namespace OneCardExpenseValidator.API.Services;

public interface IProductMatchingService
{
    /// <summary>
    /// Busca productos en la BD que coincidan con los items parseados del ticket
    /// </summary>
    Task<List<MatchedTicketItem>> MatchProductsAsync(List<ParsedTicketItem> parsedItems);
}
