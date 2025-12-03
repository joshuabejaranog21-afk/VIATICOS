using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;
using OneCardExpenseValidator.Application.Services;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICategorizationService _categorizationService;

    public ProductsController(AppDbContext context, ICategorizationService categorizationService)
    {
        _context = context;
        _categorizationService = categorizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products
            .Include(p => p.DefaultCategory)
            .Include(p => p.ProductAliases)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.DefaultCategory)
            .Include(p => p.ProductAliases)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
    {
        return await _context.Products
            .Where(p => p.DefaultCategoryId == categoryId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        product.CreatedAt = DateTime.Now;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.ProductId)
        {
            return BadRequest();
        }

        product.UpdatedAt = DateTime.Now;
        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> ProductExists(int id)
    {
        return await _context.Products.AnyAsync(p => p.ProductId == id);
    }

    // Búsqueda inteligente de productos (fuzzy search)
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<object>>> SearchProducts(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return BadRequest("La búsqueda debe tener al menos 2 caracteres");
        }

        var searchTerm = query.ToLower().Trim();

        // Buscar en productos y aliases (primero traer datos de la BD)
        var productsFromDb = await _context.Products
            .Include(p => p.DefaultCategory)
            .Include(p => p.ProductAliases)
            .Where(p => p.IsActive &&
                (p.ProductName.ToLower().Contains(searchTerm) ||
                 (p.Brand != null && p.Brand.ToLower().Contains(searchTerm)) ||
                 p.ProductAliases.Any(a => a.IsActive && a.Alias.ToLower().Contains(searchTerm))))
            .ToListAsync();

        // Calcular score y ordenar en memoria
        var products = productsFromDb
            .Select(p => new
            {
                p.ProductId,
                p.ProductName,
                p.Brand,
                p.Sku,
                CategoryName = p.DefaultCategory.CategoryName,
                CategoryId = p.DefaultCategory.CategoryId,
                IsDeductible = p.DefaultCategory.IsDeductible,
                MatchedAliases = p.ProductAliases
                    .Where(a => a.IsActive && a.Alias.ToLower().Contains(searchTerm))
                    .Select(a => a.Alias)
                    .ToList(),
                Score = CalculateMatchScore(p, searchTerm)
            })
            .OrderByDescending(p => p.Score)
            .Take(10)
            .ToList();

        return Ok(products);
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

    // Validar si un producto es deducible según políticas
    [HttpPost("validate")]
    public async Task<ActionResult<object>> ValidateProduct([FromBody] ProductValidationRequest request)
    {
        var product = await _context.Products
            .Include(p => p.DefaultCategory)
            .ThenInclude(c => c.BusinessPolicies)
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

        if (product == null)
        {
            return NotFound("Producto no encontrado");
        }

        var category = product.DefaultCategory;
        var isDeductible = category.IsDeductible ?? false;
        var validationMessages = new List<string>();
        var requiresApproval = category.RequiresApproval ?? false;

        // Verificar políticas de negocio activas para esta categoría
        var activePolicy = category.BusinessPolicies
            .Where(p => p.IsActive == true &&
                       p.EffectiveDate <= DateOnly.FromDateTime(DateTime.Now) &&
                       (p.ExpirationDate == null || p.ExpirationDate >= DateOnly.FromDateTime(DateTime.Now)))
            .FirstOrDefault();

        if (activePolicy != null)
        {
            // Verificar límites de monto
            if (activePolicy.MaxDailyAmount.HasValue && request.Amount > activePolicy.MaxDailyAmount.Value)
            {
                isDeductible = false;
                validationMessages.Add($"Excede el límite diario de {activePolicy.MaxDailyAmount:C} para {category.CategoryName}");
            }

            if (activePolicy.RequiresReceipt == true && !request.HasReceipt)
            {
                validationMessages.Add("Esta categoría requiere recibo");
            }

            if (activePolicy.RequiresManagerApproval == true ||
                (activePolicy.MinApprovalAmount.HasValue && request.Amount >= activePolicy.MinApprovalAmount.Value))
            {
                requiresApproval = true;
                validationMessages.Add("Requiere aprobación del gerente");
            }
        }

        // Verificar límite máximo de la categoría
        if (category.MaxAmountAllowed.HasValue && request.Amount > category.MaxAmountAllowed.Value)
        {
            isDeductible = false;
            validationMessages.Add($"Excede el monto máximo permitido de {category.MaxAmountAllowed:C}");
        }

        return Ok(new
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Brand = product.Brand,
            CategoryName = category.CategoryName,
            CategoryId = category.CategoryId,
            IsDeductible = isDeductible,
            RequiresApproval = requiresApproval,
            ValidationMessages = validationMessages,
            PolicyApplied = activePolicy?.PolicyName,
            Status = isDeductible ? "Aprobado" : "Rechazado"
        });
    }

    // Analizar producto con IA cuando no está en la base de datos
    [HttpPost("analyze-with-ai")]
    public async Task<ActionResult> AnalyzeProductWithAI([FromBody] AnalyzeProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            return BadRequest(new { success = false, message = "El nombre del producto es requerido" });
        }

        try
        {
            // Llamar al servicio de categorización con IA
            var result = await _categorizationService.AnalyzeProductByNameAsync(request.ProductName);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    productName = result.ProductName,
                    category = result.Category,
                    isDeductible = result.IsDeductible,
                    reason = result.Reason,
                    confidence = result.Confidence,
                    message = result.Message,
                    source = "Claude AI"
                });
            }

            return Ok(new
            {
                success = false,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Error al analizar producto: {ex.Message}"
            });
        }
    }
}

public class ProductValidationRequest
{
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
    public bool HasReceipt { get; set; } = true;
}

public class AnalyzeProductRequest
{
    public string ProductName { get; set; } = string.Empty;
}
