using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryKeywordsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryKeywordsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryKeyword>>> GetCategoryKeywords()
    {
        return await _context.CategoryKeywords
            .Include(k => k.Category)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryKeyword>> GetCategoryKeyword(int id)
    {
        var keyword = await _context.CategoryKeywords
            .Include(k => k.Category)
            .FirstOrDefaultAsync(k => k.KeywordId == id);

        if (keyword == null)
        {
            return NotFound();
        }

        return keyword;
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<CategoryKeyword>>> GetKeywordsByCategory(int categoryId)
    {
        return await _context.CategoryKeywords
            .Where(k => k.CategoryId == categoryId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<CategoryKeyword>> CreateCategoryKeyword(CategoryKeyword keyword)
    {
        keyword.CreatedAt = DateTime.Now;

        _context.CategoryKeywords.Add(keyword);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategoryKeyword), new { id = keyword.KeywordId }, keyword);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategoryKeyword(int id, CategoryKeyword keyword)
    {
        if (id != keyword.KeywordId)
        {
            return BadRequest();
        }

        _context.Entry(keyword).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CategoryKeywordExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoryKeyword(int id)
    {
        var keyword = await _context.CategoryKeywords.FindAsync(id);
        if (keyword == null)
        {
            return NotFound();
        }

        _context.CategoryKeywords.Remove(keyword);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> CategoryKeywordExists(int id)
    {
        return await _context.CategoryKeywords.AnyAsync(k => k.KeywordId == id);
    }
}
