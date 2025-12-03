using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.API.Services;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;
using System.Security.Claims;

namespace OneCardExpenseValidator.API.Controllers;

[Route("ExpenseTickets")]
[Authorize]
public class ExpenseTicketsController : Controller
{
    private readonly AppDbContext _context;
    private readonly ISpendingLimitService _spendingLimitService;

    public ExpenseTicketsController(AppDbContext context, ISpendingLimitService spendingLimitService)
    {
        _context = context;
        _spendingLimitService = spendingLimitService;
    }

    // GET: ExpenseTickets (Solo para Admin)
    [HttpGet("")]
    [HttpGet("Index")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? status, int? employeeId)
    {
        var query = _context.ExpenseTickets
            .Include(t => t.Employee)
            .ThenInclude(e => e.Department)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.ValidationStatus == status);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(t => t.EmployeeId == employeeId);
        }

        var tickets = await query
            .OrderByDescending(t => t.SubmissionDate)
            .ToListAsync();

        ViewBag.CurrentStatus = status;
        return View(tickets);
    }

    // GET: ExpenseTickets/MyTickets (Para Empleados)
    [HttpGet("MyTickets")]
    [Authorize(Roles = "Empleado")]
    public async Task<IActionResult> MyTickets(string? status)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

        if (string.IsNullOrEmpty(employeeIdClaim))
        {
            TempData["ErrorMessage"] = "No se pudo identificar tu perfil de empleado.";
            return RedirectToAction("EmployeeDashboard", "Home");
        }

        var employeeId = int.Parse(employeeIdClaim);

        var query = _context.ExpenseTickets
            .Include(t => t.ExpenseItems)
                .ThenInclude(i => i.Product)
            .Include(t => t.ExpenseItems)
                .ThenInclude(i => i.Category)
            .Where(t => t.EmployeeId == employeeId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.ValidationStatus == status);
        }

        var tickets = await query
            .OrderByDescending(t => t.SubmissionDate)
            .ToListAsync();

        ViewBag.CurrentStatus = status;
        ViewBag.EmployeeId = employeeId;

        return View(tickets);
    }

    // GET: ExpenseTickets/Details/5
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticket = await _context.ExpenseTickets
            .Include(t => t.Employee)
            .ThenInclude(e => e.Department)
            .Include(t => t.ApprovedByNavigation)
            .Include(t => t.ExpenseItems)
                .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(m => m.TicketId == id);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    // GET: ExpenseTickets/Create
    [HttpGet("Create")]
    public IActionResult Create(int? employeeId)
    {
        // Si es empleado, solo puede crear para sí mismo
        if (User.IsInRole("Empleado"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (!string.IsNullOrEmpty(employeeIdClaim))
            {
                ViewBag.EmployeeId = int.Parse(employeeIdClaim);
                ViewBag.IsEmployee = true;
            }
        }
        else if (User.IsInRole("Admin"))
        {
            // Admin puede crear para cualquier empleado
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", employeeId);
            ViewBag.IsEmployee = false;
        }

        return View();
    }

    // POST: ExpenseTickets/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TicketNumber,EmployeeId,TicketDate,Vendor,TotalAmount,Notes")] ExpenseTicket ticket, IFormFile? ticketImage)
    {
        // Validar que empleados solo creen tickets para sí mismos
        if (User.IsInRole("Empleado"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (!string.IsNullOrEmpty(employeeIdClaim))
            {
                var loggedEmployeeId = int.Parse(employeeIdClaim);
                if (ticket.EmployeeId != loggedEmployeeId)
                {
                    TempData["ErrorMessage"] = "Solo puedes crear tickets para ti mismo.";
                    return RedirectToAction("EmployeeDashboard", "Home");
                }
            }
        }

        if (ModelState.IsValid)
        {
            // Validar que EmployeeId no sea null
            if (!ticket.EmployeeId.HasValue)
            {
                TempData["ErrorMessage"] = "El ID del empleado es requerido.";
                return View(ticket);
            }

            // Validar que el empleado existe
            var employee = await _context.Employees.FindAsync(ticket.EmployeeId.Value);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "El empleado no existe.";
                return View(ticket);
            }

            // Validar límite diario
            var exceedsDailyLimit = await _spendingLimitService.WillExceedDailyLimitAsync(ticket.EmployeeId.Value, ticket.TotalAmount);
            if (exceedsDailyLimit)
            {
                var currentDailySpending = await _spendingLimitService.GetDailySpendingAsync(ticket.EmployeeId.Value);
                var dailyLimit = employee.DailyExpenseLimit ?? 0m;
                var availableAmount = dailyLimit - currentDailySpending;

                TempData["ErrorMessage"] = $"No se puede crear el ticket. Se excede el límite de gasto diario. Límite: ${dailyLimit:N2}, Gasto actual: ${currentDailySpending:N2}, Disponible: ${availableAmount:N2}";

                // Volver a cargar los datos para la vista
                if (User.IsInRole("Empleado"))
                {
                    var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
                    if (!string.IsNullOrEmpty(employeeIdClaim))
                    {
                        ViewBag.EmployeeId = int.Parse(employeeIdClaim);
                        ViewBag.IsEmployee = true;
                    }
                }
                else
                {
                    ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
                    ViewBag.IsEmployee = false;
                }

                return View(ticket);
            }

            // Validar límite mensual
            var exceedsMonthlyLimit = await _spendingLimitService.WillExceedMonthlyLimitAsync(ticket.EmployeeId.Value, ticket.TotalAmount);
            if (exceedsMonthlyLimit)
            {
                var currentMonthlySpending = await _spendingLimitService.GetMonthlySpendingAsync(ticket.EmployeeId.Value);
                var monthlyLimit = employee.MonthlyExpenseLimit ?? 0m;
                var availableAmount = monthlyLimit - currentMonthlySpending;

                TempData["ErrorMessage"] = $"No se puede crear el ticket. Se excede el límite de gasto mensual. Límite: ${monthlyLimit:N2}, Gasto actual: ${currentMonthlySpending:N2}, Disponible: ${availableAmount:N2}";

                // Volver a cargar los datos para la vista
                if (User.IsInRole("Empleado"))
                {
                    var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
                    if (!string.IsNullOrEmpty(employeeIdClaim))
                    {
                        ViewBag.EmployeeId = int.Parse(employeeIdClaim);
                        ViewBag.IsEmployee = true;
                    }
                }
                else
                {
                    ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
                    ViewBag.IsEmployee = false;
                }

                return View(ticket);
            }

            // Manejar la subida de imagen
            if (ticketImage != null && ticketImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");

                // Crear la carpeta si no existe
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generar un nombre único para el archivo
                var uniqueFileName = $"{Guid.NewGuid()}_{ticketImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Guardar el archivo
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ticketImage.CopyToAsync(fileStream);
                }

                // Guardar la ruta relativa en la base de datos
                ticket.TicketImagePath = $"/uploads/tickets/{uniqueFileName}";
            }

            ticket.SubmissionDate = DateTime.Now;
            ticket.ValidationStatus = "Pending";
            ticket.CreatedAt = DateTime.Now;
            ticket.UpdatedAt = DateTime.Now;

            _context.Add(ticket);
            await _context.SaveChangesAsync();

            // Verificar y enviar notificaciones si está cerca del límite
            if (ticket.EmployeeId.HasValue)
            {
                await _spendingLimitService.CheckAndNotifySpendingLimitAsync(ticket.EmployeeId.Value);
            }

            TempData["SuccessMessage"] = "Ticket de gasto creado exitosamente.";

            // Redirigir según el rol
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(MyTickets));
            }
        }

        // Volver a cargar los datos necesarios para la vista
        if (User.IsInRole("Empleado"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (!string.IsNullOrEmpty(employeeIdClaim))
            {
                ViewBag.EmployeeId = int.Parse(employeeIdClaim);
                ViewBag.IsEmployee = true;
            }
        }
        else
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
            ViewBag.IsEmployee = false;
        }

        return View(ticket);
    }

    // GET: ExpenseTickets/Edit/5
    [HttpGet("Edit/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticket = await _context.ExpenseTickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }
        ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
        return View(ticket);
    }

    // POST: ExpenseTickets/Edit/5
    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("TicketId,TicketNumber,EmployeeId,SubmissionDate,TicketDate,Vendor,TotalAmount,ValidationStatus,Notes,CreatedAt")] ExpenseTicket ticket)
    {
        if (id != ticket.TicketId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Obtener el ticket original para comparar montos
            var originalTicket = await _context.ExpenseTickets.AsNoTracking().FirstOrDefaultAsync(t => t.TicketId == id);
            if (originalTicket == null)
            {
                return NotFound();
            }

            // Si se está actualizando el monto, validar límites
            if (ticket.TotalAmount != originalTicket.TotalAmount)
            {
                if (!ticket.EmployeeId.HasValue)
                {
                    TempData["ErrorMessage"] = "El ticket no tiene un empleado asignado.";
                    ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
                    return View(ticket);
                }

                var employee = await _context.Employees.FindAsync(ticket.EmployeeId.Value);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "El empleado no existe.";
                    ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
                    return View(ticket);
                }

                var amountDifference = ticket.TotalAmount - originalTicket.TotalAmount;

                // Solo validar si el nuevo monto es mayor al actual
                if (amountDifference > 0)
                {
                    // Validar límite diario
                    var exceedsDailyLimit = await _spendingLimitService.WillExceedDailyLimitAsync(ticket.EmployeeId.Value, amountDifference);
                    if (exceedsDailyLimit)
                    {
                        var currentDailySpending = await _spendingLimitService.GetDailySpendingAsync(ticket.EmployeeId.Value);
                        var dailyLimit = employee.DailyExpenseLimit ?? 0m;
                        var availableAmount = dailyLimit - currentDailySpending;

                        TempData["ErrorMessage"] = $"No se puede actualizar el monto. Se excede el límite de gasto diario. Límite: ${dailyLimit:N2}, Gasto actual: ${currentDailySpending:N2}, Disponible: ${availableAmount:N2}";
                        ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
                        return View(ticket);
                    }

                    // Validar límite mensual
                    var exceedsMonthlyLimit = await _spendingLimitService.WillExceedMonthlyLimitAsync(ticket.EmployeeId.Value, amountDifference);
                    if (exceedsMonthlyLimit)
                    {
                        var currentMonthlySpending = await _spendingLimitService.GetMonthlySpendingAsync(ticket.EmployeeId.Value);
                        var monthlyLimit = employee.MonthlyExpenseLimit ?? 0m;
                        var availableAmount = monthlyLimit - currentMonthlySpending;

                        TempData["ErrorMessage"] = $"No se puede actualizar el monto. Se excede el límite de gasto mensual. Límite: ${monthlyLimit:N2}, Gasto actual: ${currentMonthlySpending:N2}, Disponible: ${availableAmount:N2}";
                        ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
                        return View(ticket);
                    }
                }
            }

            try
            {
                ticket.UpdatedAt = DateTime.Now;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ticket actualizado exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(ticket.TicketId))
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
        ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName", ticket.EmployeeId);
        return View(ticket);
    }

    // GET: ExpenseTickets/Approve/5
    [HttpGet("Approve/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticket = await _context.ExpenseTickets
            .Include(t => t.Employee)
            .ThenInclude(e => e.Department)
            .Include(t => t.ExpenseItems)
                .ThenInclude(i => i.Product)
            .Include(t => t.ExpenseItems)
                .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(t => t.TicketId == id);

        if (ticket == null)
        {
            return NotFound();
        }

        // Calcular montos deducibles y no deducibles
        if (ticket.ExpenseItems != null && ticket.ExpenseItems.Any())
        {
            var deductibleAmount = ticket.ExpenseItems
                .Where(i => i.IsDeductible == true)
                .Sum(i => i.TotalPrice);

            var nonDeductibleAmount = ticket.ExpenseItems
                .Where(i => i.IsDeductible == false)
                .Sum(i => i.TotalPrice);

            ViewBag.DeductibleAmount = deductibleAmount;
            ViewBag.NonDeductibleAmount = nonDeductibleAmount;
            ViewBag.HasNonDeductibleItems = nonDeductibleAmount > 0;
        }

        return View(ticket);
    }

    // POST: ExpenseTickets/Approve/5
    [HttpPost("Approve/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveConfirmed(int id)
    {
        // Obtener el EmployeeId del usuario en sesión
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

        if (string.IsNullOrEmpty(employeeIdClaim))
        {
            TempData["ErrorMessage"] = "No se pudo identificar tu perfil de empleado.";
            return RedirectToAction(nameof(Index));
        }

        int approvedBy = int.Parse(employeeIdClaim);

        var ticket = await _context.ExpenseTickets
            .Include(t => t.ExpenseItems)
            .FirstOrDefaultAsync(t => t.TicketId == id);

        if (ticket == null)
        {
            return NotFound();
        }

        // Calcular montos deducibles y no deducibles
        if (ticket.ExpenseItems != null && ticket.ExpenseItems.Any())
        {
            ticket.DeductibleAmount = ticket.ExpenseItems
                .Where(i => i.IsDeductible == true)
                .Sum(i => i.TotalPrice);

            ticket.NonDeductibleAmount = ticket.ExpenseItems
                .Where(i => i.IsDeductible == false)
                .Sum(i => i.TotalPrice);
        }
        else
        {
            // Si no hay items, asumir que todo es deducible
            ticket.DeductibleAmount = ticket.TotalAmount;
            ticket.NonDeductibleAmount = 0;
        }

        ticket.ValidationStatus = "Approved";
        ticket.ApprovedBy = approvedBy;
        ticket.ApprovalDate = DateTime.Now;
        ticket.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        // Verificar y enviar notificaciones si está cerca del límite
        if (ticket.EmployeeId.HasValue)
        {
            await _spendingLimitService.CheckAndNotifySpendingLimitAsync(ticket.EmployeeId.Value);
        }

        TempData["SuccessMessage"] = "Ticket aprobado exitosamente.";

        return RedirectToAction(nameof(Index));
    }

    // GET: ExpenseTickets/Reject/5
    [HttpGet("Reject/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticket = await _context.ExpenseTickets
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.TicketId == id);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    // POST: ExpenseTickets/Reject/5
    [HttpPost("Reject/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectConfirmed(int id, string rejectionReason)
    {
        var ticket = await _context.ExpenseTickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        ticket.ValidationStatus = "Rejected";
        ticket.RejectionReason = rejectionReason;
        ticket.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Ticket rechazado.";

        return RedirectToAction(nameof(Index));
    }

    // GET: ExpenseTickets/Delete/5
    [HttpGet("Delete/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticket = await _context.ExpenseTickets
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(m => m.TicketId == id);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    // POST: ExpenseTickets/Delete/5
    [HttpPost("Delete/{id}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ticket = await _context.ExpenseTickets.FindAsync(id);
        if (ticket != null)
        {
            _context.ExpenseTickets.Remove(ticket);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ticket eliminado exitosamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool TicketExists(int id)
    {
        return _context.ExpenseTickets.Any(e => e.TicketId == id);
    }
}
