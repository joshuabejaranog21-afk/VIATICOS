using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.API.Models;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Services;

public class ProductMatchingService : IProductMatchingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductMatchingService> _logger;

    public ProductMatchingService(AppDbContext context, ILogger<ProductMatchingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MatchedTicketItem>> MatchProductsAsync(List<ParsedTicketItem> parsedItems)
    {
        var matchedItems = new List<MatchedTicketItem>();

        foreach (var item in parsedItems)
        {
            var matchedItem = await FindBestProductMatchAsync(item);
            matchedItems.Add(matchedItem);
        }

        return matchedItems;
    }

    private async Task<MatchedTicketItem> FindBestProductMatchAsync(ParsedTicketItem parsedItem)
    {
        var searchTerm = parsedItem.Description.ToLower().Trim();

        _logger.LogInformation($"Searching product match for: {searchTerm}");

        // Buscar productos que coincidan con la descripción
        var productsFromDb = await _context.Products
            .Include(p => p.DefaultCategory)
            .Include(p => p.ProductAliases)
            .Where(p => p.IsActive &&
                (p.ProductName.ToLower().Contains(searchTerm) ||
                 (p.Brand != null && p.Brand.ToLower().Contains(searchTerm)) ||
                 p.ProductAliases.Any(a => a.IsActive && a.Alias.ToLower().Contains(searchTerm))))
            .ToListAsync();

        if (productsFromDb.Count == 0)
        {
            _logger.LogWarning($"No product match found for: {searchTerm}");
            return new MatchedTicketItem
            {
                OriginalItem = parsedItem,
                MatchScore = 0
            };
        }

        // Calcular score para cada producto y elegir el mejor
        var productsWithScores = productsFromDb
            .Select(p => new
            {
                Product = p,
                Score = CalculateMatchScore(p, searchTerm)
            })
            .OrderByDescending(x => x.Score)
            .First();

        _logger.LogInformation($"Best match for '{searchTerm}': {productsWithScores.Product.ProductName} (Score: {productsWithScores.Score})");

        return new MatchedTicketItem
        {
            OriginalItem = parsedItem,
            ProductId = productsWithScores.Product.ProductId,
            ProductName = productsWithScores.Product.ProductName,
            CategoryId = productsWithScores.Product.DefaultCategory?.CategoryId,
            CategoryName = productsWithScores.Product.DefaultCategory?.CategoryName,
            IsDeductible = productsWithScores.Product.DefaultCategory?.IsDeductible,
            MatchScore = productsWithScores.Score
        };
    }

    private static int CalculateMatchScore(Product product, string searchTerm)
    {
        int score = 0;
        var productNameLower = product.ProductName.ToLower();
        var brandLower = product.Brand?.ToLower() ?? "";

        // Coincidencia exacta = mayor score
        if (productNameLower == searchTerm) score += 100;
        else if (productNameLower.StartsWith(searchTerm)) score += 50;
        else if (productNameLower.Contains(searchTerm)) score += 25;

        if (brandLower == searchTerm) score += 80;
        else if (brandLower.StartsWith(searchTerm)) score += 40;
        else if (brandLower.Contains(searchTerm)) score += 20;

        // Bonificación por aliases
        foreach (var alias in product.ProductAliases.Where(a => a.IsActive))
        {
            var aliasLower = alias.Alias.ToLower();
            if (aliasLower == searchTerm) score += 90;
            else if (aliasLower.StartsWith(searchTerm)) score += 45;
            else if (aliasLower.Contains(searchTerm)) score += 22;
        }

        return score;
    }
}
