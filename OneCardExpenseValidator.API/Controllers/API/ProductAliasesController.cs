using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductAliasesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductAliasesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductAlias>>> GetProductAliases()
    {
        return await _context.ProductAliases
            .Include(a => a.Product)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductAlias>> GetProductAlias(int id)
    {
        var alias = await _context.ProductAliases
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.AliasId == id);

        if (alias == null)
        {
            return NotFound();
        }

        return alias;
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ProductAlias>>> GetAliasesByProduct(int productId)
    {
        return await _context.ProductAliases
            .Where(a => a.ProductId == productId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ProductAlias>> CreateProductAlias(ProductAlias alias)
    {
        alias.CreatedAt = DateTime.Now;

        _context.ProductAliases.Add(alias);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductAlias), new { id = alias.AliasId }, alias);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductAlias(int id, ProductAlias alias)
    {
        if (id != alias.AliasId)
        {
            return BadRequest();
        }

        _context.Entry(alias).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductAliasExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductAlias(int id)
    {
        var alias = await _context.ProductAliases.FindAsync(id);
        if (alias == null)
        {
            return NotFound();
        }

        _context.ProductAliases.Remove(alias);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> ProductAliasExists(int id)
    {
        return await _context.ProductAliases.AnyAsync(a => a.AliasId == id);
    }
}
