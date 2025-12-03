using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using System.Security.Claims;

namespace OneCardExpenseValidator.API.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        // Obtener estadísticas para el dashboard de Admin
        var totalTickets = await _context.ExpenseTickets.CountAsync();
        var pendingTickets = await _context.ExpenseTickets
            .CountAsync(t => t.ValidationStatus == "Pending");
        var approvedTickets = await _context.ExpenseTickets
            .CountAsync(t => t.ValidationStatus == "Approved");
        var rejectedTickets = await _context.ExpenseTickets
            .CountAsync(t => t.ValidationStatus == "Rejected");

        var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive == true);
        var totalDepartments = await _context.Departments.CountAsync();

        // Tickets recientes
        var recentTickets = await _context.ExpenseTickets
            .Include(t => t.Employee)
            .OrderByDescending(t => t.SubmissionDate)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalTickets = totalTickets;
        ViewBag.PendingTickets = pendingTickets;
        ViewBag.ApprovedTickets = approvedTickets;
        ViewBag.RejectedTickets = rejectedTickets;
        ViewBag.TotalEmployees = totalEmployees;
        ViewBag.TotalDepartments = totalDepartments;
        ViewBag.RecentTickets = recentTickets;

        return View();
    }

    [Authorize(Roles = "Empleado")]
    public async Task<IActionResult> EmployeeDashboard()
    {
        // Obtener el EmployeeId del usuario logueado
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

        if (string.IsNullOrEmpty(employeeIdClaim))
        {
            TempData["ErrorMessage"] = "No se pudo identificar tu perfil de empleado.";
            return RedirectToAction("Login", "Auth");
        }

        var employeeId = int.Parse(employeeIdClaim);

        // Obtener datos del empleado
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "No se encontró tu perfil de empleado.";
            return RedirectToAction("Login", "Auth");
        }

        // Obtener estadísticas de los tickets del empleado
        var myTickets = await _context.ExpenseTickets
            .Where(t => t.EmployeeId == employeeId)
            .ToListAsync();

        var totalMyTickets = myTickets.Count;
        var pendingMyTickets = myTickets.Count(t => t.ValidationStatus == "Pending");
        var approvedMyTickets = myTickets.Count(t => t.ValidationStatus == "Approved");
        var rejectedMyTickets = myTickets.Count(t => t.ValidationStatus == "Rejected");

        // Tickets recientes del empleado
        var recentMyTickets = await _context.ExpenseTickets
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.SubmissionDate)
            .Take(5)
            .ToListAsync();

        // Calcular totales de gastos del mes actual
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var monthlyExpenses = myTickets
            .Where(t => t.SubmissionDate.HasValue &&
                       t.SubmissionDate.Value.Month == currentMonth &&
                       t.SubmissionDate.Value.Year == currentYear &&
                       t.ValidationStatus == "Approved")
            .Sum(t => t.TotalAmount);

        var dailyExpenses = myTickets
            .Where(t => t.SubmissionDate.HasValue &&
                       t.SubmissionDate.Value.Date == DateTime.Today &&
                       t.ValidationStatus == "Approved")
            .Sum(t => t.TotalAmount);

        ViewBag.Employee = employee;
        ViewBag.TotalMyTickets = totalMyTickets;
        ViewBag.PendingMyTickets = pendingMyTickets;
        ViewBag.ApprovedMyTickets = approvedMyTickets;
        ViewBag.RejectedMyTickets = rejectedMyTickets;
        ViewBag.RecentMyTickets = recentMyTickets;
        ViewBag.MonthlyExpenses = monthlyExpenses;
        ViewBag.DailyExpenses = dailyExpenses;
        ViewBag.DailyLimit = employee.DailyExpenseLimit;
        ViewBag.MonthlyLimit = employee.MonthlyExpenseLimit;

        return View();
    }

    [Authorize(Roles = "Empleado")]
    public IActionResult PhoneScanner()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult TicketAnalyzer()
    {
        return View();
    }
}
