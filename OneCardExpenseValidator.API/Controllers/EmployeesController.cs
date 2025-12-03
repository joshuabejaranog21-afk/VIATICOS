using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[Route("Employees")]
[Authorize(Roles = "Admin")]
public class EmployeesController : Controller
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Employees
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .OrderBy(e => e.LastName)
            .ToListAsync();
        return View(employees);
    }

    // GET: Employees/Details/5
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(m => m.EmployeeId == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // GET: Employees/Create
    [HttpGet("Create")]
    public IActionResult Create()
    {
        ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(d => d.IsActive == true), "DepartmentId", "DepartmentName");
        return View();
    }

    // POST: Employees/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EmployeeCode,FirstName,LastName,Email,DepartmentId,Position,DailyExpenseLimit,MonthlyExpenseLimit")] Employee employee)
    {
        if (ModelState.IsValid)
        {
            employee.CreatedAt = DateTime.Now;
            employee.IsActive = true;
            _context.Add(employee);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Empleado creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(d => d.IsActive == true), "DepartmentId", "DepartmentName", employee.DepartmentId);
        return View(employee);
    }

    // GET: Employees/Edit/5
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(d => d.IsActive == true), "DepartmentId", "DepartmentName", employee.DepartmentId);
        return View(employee);
    }

    // POST: Employees/Edit/5
    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,EmployeeCode,FirstName,LastName,Email,DepartmentId,Position,DailyExpenseLimit,MonthlyExpenseLimit,IsActive,CreatedAt")] Employee employee)
    {
        if (id != employee.EmployeeId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Empleado actualizado exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.EmployeeId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(d => d.IsActive == true), "DepartmentId", "DepartmentName", employee.DepartmentId);
        return View(employee);
    }

    // GET: Employees/Delete/5
    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(m => m.EmployeeId == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // POST: Employees/Delete/5
    [HttpPost("Delete/{id}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            // Soft delete - marcar como inactivo en lugar de eliminar
            employee.IsActive = false;
            _context.Update(employee);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Empleado desactivado exitosamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.EmployeeId == id);
    }
}
