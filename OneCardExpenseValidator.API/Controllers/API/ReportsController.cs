using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<IEnumerable<VwExpenseDashboard>>> GetDashboard()
    {
        return await _context.VwExpenseDashboards.ToListAsync();
    }

    [HttpGet("pending-expenses")]
    public async Task<ActionResult<IEnumerable<VwPendingExpense>>> GetPendingExpenses()
    {
        return await _context.VwPendingExpenses.ToListAsync();
    }

    [HttpGet("top-categories")]
    public async Task<ActionResult<IEnumerable<VwTopCategory>>> GetTopCategories()
    {
        return await _context.VwTopCategories.ToListAsync();
    }

    [HttpGet("monthly-reports")]
    public async Task<ActionResult<IEnumerable<MonthlyExpenseReport>>> GetMonthlyReports()
    {
        return await _context.MonthlyExpenseReports
            .Include(r => r.Employee)
            .ToListAsync();
    }

    [HttpGet("monthly-reports/{id}")]
    public async Task<ActionResult<MonthlyExpenseReport>> GetMonthlyReport(int id)
    {
        var report = await _context.MonthlyExpenseReports
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound();
        }

        return report;
    }

    [HttpGet("monthly-reports/employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<MonthlyExpenseReport>>> GetReportsByEmployee(int employeeId)
    {
        return await _context.MonthlyExpenseReports
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.ReportYear)
            .ThenByDescending(r => r.ReportMonth)
            .ToListAsync();
    }
}
