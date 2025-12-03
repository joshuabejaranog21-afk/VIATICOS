using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;
using OneCardExpenseValidator.API.Services;

namespace OneCardExpenseValidator.API.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class ExpenseTicketsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISpendingLimitService _spendingLimitService;

    public ExpenseTicketsController(AppDbContext context, ISpendingLimitService spendingLimitService)
    {
        _context = context;
        _spendingLimitService = spendingLimitService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseTicket>>> GetExpenseTickets()
    {
        return await _context.ExpenseTickets
            .Include(t => t.Employee)
            .Include(t => t.ExpenseItems)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseTicket>> GetExpenseTicket(int id)
    {
        var ticket = await _context.ExpenseTickets
            .Include(t => t.Employee)
            .Include(t => t.ApprovedByNavigation)
            .Include(t => t.ExpenseItems)
                .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(t => t.TicketId == id);

        if (ticket == null)
        {
            return NotFound();
        }

        return ticket;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<ExpenseTicket>>> GetPendingTickets()
    {
        return await _context.ExpenseTickets
            .Include(t => t.Employee)
            .Where(t => t.ValidationStatus == "Pending")
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseTicket>> CreateExpenseTicket(ExpenseTicket ticket)
    {
        // Validar que EmployeeId no sea null
        if (!ticket.EmployeeId.HasValue)
        {
            return BadRequest(new {
                success = false,
                message = "El ID del empleado es requerido"
            });
        }

        // Validar que el empleado existe
        var employee = await _context.Employees.FindAsync(ticket.EmployeeId.Value);
        if (employee == null)
        {
            return BadRequest(new {
                success = false,
                message = "El empleado no existe"
            });
        }

        // Validar límite diario
        var exceedsDailyLimit = await _spendingLimitService.WillExceedDailyLimitAsync(ticket.EmployeeId.Value, ticket.TotalAmount);
        if (exceedsDailyLimit)
        {
            var currentDailySpending = await _spendingLimitService.GetDailySpendingAsync(ticket.EmployeeId.Value);
            var dailyLimit = employee.DailyExpenseLimit ?? 0m;

            return BadRequest(new {
                success = false,
                message = "No se puede crear el ticket. Se excede el límite de gasto diario.",
                limitType = "daily",
                currentSpending = currentDailySpending,
                limit = dailyLimit,
                attemptedAmount = ticket.TotalAmount,
                availableAmount = dailyLimit - currentDailySpending
            });
        }

        // Validar límite mensual
        var exceedsMonthlyLimit = await _spendingLimitService.WillExceedMonthlyLimitAsync(ticket.EmployeeId.Value, ticket.TotalAmount);
        if (exceedsMonthlyLimit)
        {
            var currentMonthlySpending = await _spendingLimitService.GetMonthlySpendingAsync(ticket.EmployeeId.Value);
            var monthlyLimit = employee.MonthlyExpenseLimit ?? 0m;

            return BadRequest(new {
                success = false,
                message = "No se puede crear el ticket. Se excede el límite de gasto mensual.",
                limitType = "monthly",
                currentSpending = currentMonthlySpending,
                limit = monthlyLimit,
                attemptedAmount = ticket.TotalAmount,
                availableAmount = monthlyLimit - currentMonthlySpending
            });
        }

        // Si pasa las validaciones, crear el ticket
        ticket.CreatedAt = DateTime.Now;
        ticket.SubmissionDate = DateTime.Now;
        ticket.ValidationStatus = "Pending";

        _context.ExpenseTickets.Add(ticket);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExpenseTicket), new { id = ticket.TicketId }, ticket);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpenseTicket(int id, ExpenseTicket ticket)
    {
        if (id != ticket.TicketId)
        {
            return BadRequest();
        }

        ticket.UpdatedAt = DateTime.Now;
        _context.Entry(ticket).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExpenseTicketExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveTicket(int id, [FromBody] int approvedBy)
    {
        var ticket = await _context.ExpenseTickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        ticket.ValidationStatus = "Approved";
        ticket.ApprovedBy = approvedBy;
        ticket.ApprovalDate = DateTime.Now;
        ticket.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectTicket(int id, [FromBody] string reason)
    {
        var ticket = await _context.ExpenseTickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        ticket.ValidationStatus = "Rejected";
        ticket.RejectionReason = reason;
        ticket.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpenseTicket(int id)
    {
        var ticket = await _context.ExpenseTickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        _context.ExpenseTickets.Remove(ticket);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/update-field")]
    public async Task<IActionResult> UpdateTicketField(int id, [FromBody] UpdateTicketFieldRequest request)
    {
        var ticket = await _context.ExpenseTickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound("Ticket no encontrado");
        }

        // Solo permitir edición si el ticket está pendiente
        if (ticket.ValidationStatus != "Pending")
        {
            return BadRequest("Solo se pueden editar tickets pendientes");
        }

        // Si se está actualizando el monto, validar límites
        if (request.TotalAmount.HasValue && request.TotalAmount.Value != ticket.TotalAmount)
        {
            if (!ticket.EmployeeId.HasValue)
            {
                return BadRequest("El ticket no tiene un empleado asignado");
            }

            var employee = await _context.Employees.FindAsync(ticket.EmployeeId.Value);
            if (employee == null)
            {
                return BadRequest("El empleado no existe");
            }

            // Calcular la diferencia de monto (cuánto más o menos se está gastando)
            var currentTicketAmount = ticket.TotalAmount;
            var newTicketAmount = request.TotalAmount.Value;
            var amountDifference = newTicketAmount - currentTicketAmount;

            // Solo validar si el nuevo monto es mayor al actual
            if (amountDifference > 0)
            {
                // Validar límite diario
                var exceedsDailyLimit = await _spendingLimitService.WillExceedDailyLimitAsync(ticket.EmployeeId.Value, amountDifference);
                if (exceedsDailyLimit)
                {
                    var currentDailySpending = await _spendingLimitService.GetDailySpendingAsync(ticket.EmployeeId.Value);
                    var dailyLimit = employee.DailyExpenseLimit ?? 0m;

                    return BadRequest(new {
                        success = false,
                        message = "No se puede actualizar el monto. Se excede el límite de gasto diario.",
                        limitType = "daily",
                        currentSpending = currentDailySpending,
                        limit = dailyLimit,
                        currentTicketAmount = currentTicketAmount,
                        attemptedAmount = newTicketAmount,
                        availableAmount = dailyLimit - currentDailySpending
                    });
                }

                // Validar límite mensual
                var exceedsMonthlyLimit = await _spendingLimitService.WillExceedMonthlyLimitAsync(ticket.EmployeeId.Value, amountDifference);
                if (exceedsMonthlyLimit)
                {
                    var currentMonthlySpending = await _spendingLimitService.GetMonthlySpendingAsync(ticket.EmployeeId.Value);
                    var monthlyLimit = employee.MonthlyExpenseLimit ?? 0m;

                    return BadRequest(new {
                        success = false,
                        message = "No se puede actualizar el monto. Se excede el límite de gasto mensual.",
                        limitType = "monthly",
                        currentSpending = currentMonthlySpending,
                        limit = monthlyLimit,
                        currentTicketAmount = currentTicketAmount,
                        attemptedAmount = newTicketAmount,
                        availableAmount = monthlyLimit - currentMonthlySpending
                    });
                }
            }

            ticket.TotalAmount = request.TotalAmount.Value;
        }

        // Actualizar los demás campos
        if (request.Vendor != null)
        {
            ticket.Vendor = request.Vendor;
        }

        if (request.TicketDate.HasValue)
        {
            ticket.TicketDate = DateOnly.FromDateTime(request.TicketDate.Value);
        }

        if (request.Notes != null)
        {
            ticket.Notes = request.Notes;
        }

        ticket.UpdatedAt = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExpenseTicketExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return Ok(new
        {
            message = "Campo actualizado correctamente",
            ticketId = ticket.TicketId
        });
    }

    private async Task<bool> ExpenseTicketExists(int id)
    {
        return await _context.ExpenseTickets.AnyAsync(t => t.TicketId == id);
    }
}

public class UpdateTicketFieldRequest
{
    public string? Vendor { get; set; }
    public DateTime? TicketDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Notes { get; set; }
}
