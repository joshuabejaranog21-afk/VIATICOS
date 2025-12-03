using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;

namespace OneCardExpenseValidator.API.Controllers;

[Route("Employee")]
[Authorize(Roles = "Empleado")]
public class EmployeeController : Controller
{
    private readonly AppDbContext _context;

    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Employee/Profile
    [HttpGet("Profile")]
    public async Task<IActionResult> Profile()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

        if (string.IsNullOrEmpty(employeeIdClaim))
        {
            TempData["ErrorMessage"] = "No se pudo identificar tu perfil de empleado.";
            return RedirectToAction("EmployeeDashboard", "Home");
        }

        var employeeId = int.Parse(employeeIdClaim);

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.ExpenseTicketEmployees)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "No se encontró tu perfil de empleado.";
            return RedirectToAction("EmployeeDashboard", "Home");
        }

        // Calcular estadísticas del empleado
        var totalTickets = employee.ExpenseTicketEmployees.Count;
        var totalApproved = employee.ExpenseTicketEmployees.Count(t => t.ValidationStatus == "Approved");
        var totalRejected = employee.ExpenseTicketEmployees.Count(t => t.ValidationStatus == "Rejected");
        var totalPending = employee.ExpenseTicketEmployees.Count(t => t.ValidationStatus == "Pending");

        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var monthlyExpenses = employee.ExpenseTicketEmployees
            .Where(t => t.SubmissionDate.HasValue &&
                       t.SubmissionDate.Value.Month == currentMonth &&
                       t.SubmissionDate.Value.Year == currentYear &&
                       t.ValidationStatus == "Approved")
            .Sum(t => t.TotalAmount);

        var dailyExpenses = employee.ExpenseTicketEmployees
            .Where(t => t.SubmissionDate.HasValue &&
                       t.SubmissionDate.Value.Date == DateTime.Today &&
                       t.ValidationStatus == "Approved")
            .Sum(t => t.TotalAmount);

        ViewBag.TotalTickets = totalTickets;
        ViewBag.TotalApproved = totalApproved;
        ViewBag.TotalRejected = totalRejected;
        ViewBag.TotalPending = totalPending;
        ViewBag.MonthlyExpenses = monthlyExpenses;
        ViewBag.DailyExpenses = dailyExpenses;

        return View(employee);
    }
}
