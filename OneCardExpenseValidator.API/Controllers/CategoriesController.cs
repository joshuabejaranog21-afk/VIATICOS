using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[Route("Categories")]
[Authorize]
public class CategoriesController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories
            .Include(c => c.CategoryKeywords)
            .FirstOrDefaultAsync(m => m.CategoryId == id);

        if (category == null) return NotFound();

        return View(category);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CategoryCode,CategoryName,Description,IsDeductible,RequiresApproval,MaxAmountAllowed")] Category category)
    {
        if (ModelState.IsValid)
        {
            category.CreatedAt = DateTime.Now;
            category.UpdatedAt = DateTime.Now;
            _context.Add(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Categoría creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryCode,CategoryName,Description,IsDeductible,RequiresApproval,MaxAmountAllowed,CreatedAt")] Category category)
    {
        if (id != category.CategoryId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                category.UpdatedAt = DateTime.Now;
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Categoría actualizada exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.CategoryId == category.CategoryId))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }
}
