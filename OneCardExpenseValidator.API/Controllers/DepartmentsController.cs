using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[Route("Departments")]
[Authorize(Roles = "Admin")]
public class DepartmentsController : Controller
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .Include(d => d.Employees)
            .ToListAsync();
        return View(departments);
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(m => m.DepartmentId == id);

        if (department == null) return NotFound();

        return View(department);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DepartmentCode,DepartmentName,BudgetLimit")] Department department)
    {
        if (ModelState.IsValid)
        {
            department.IsActive = true;
            _context.Add(department);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Departamento creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var department = await _context.Departments.FindAsync(id);
        if (department == null) return NotFound();
        return View(department);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,DepartmentCode,DepartmentName,BudgetLimit,IsActive")] Department department)
    {
        if (id != department.DepartmentId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Departamento actualizado exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Departments.Any(e => e.DepartmentId == department.DepartmentId))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }
}
