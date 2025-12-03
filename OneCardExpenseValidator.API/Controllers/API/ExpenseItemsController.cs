using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExpenseItemsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseItem>>> GetExpenseItems()
    {
        return await _context.ExpenseItems
            .Include(i => i.Ticket)
            .Include(i => i.Category)
            .Include(i => i.Product)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseItem>> GetExpenseItem(int id)
    {
        var item = await _context.ExpenseItems
            .Include(i => i.Ticket)
            .Include(i => i.Category)
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ItemId == id);

        if (item == null)
        {
            return NotFound();
        }

        return item;
    }

    [HttpGet("ticket/{ticketId}")]
    public async Task<ActionResult<IEnumerable<ExpenseItem>>> GetItemsByTicket(int ticketId)
    {
        return await _context.ExpenseItems
            .Include(i => i.Category)
            .Include(i => i.Product)
            .Where(i => i.TicketId == ticketId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseItem>> CreateExpenseItem(ExpenseItem item)
    {
        _context.ExpenseItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExpenseItem), new { id = item.ItemId }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpenseItem(int id, [FromBody] UpdateExpenseItemRequest request)
    {
        if (id != request.ItemId)
        {
            return BadRequest("ID mismatch");
        }

        // Obtener el item existente con sus relaciones
        var existingItem = await _context.ExpenseItems
            .Include(i => i.Product)
            .ThenInclude(p => p.DefaultCategory)
            .ThenInclude(c => c.BusinessPolicies)
            .FirstOrDefaultAsync(i => i.ItemId == id);

        if (existingItem == null)
        {
            return NotFound("Item no encontrado");
        }

        // Actualizar campos editables
        existingItem.Quantity = request.Quantity;
        existingItem.UnitPrice = request.UnitPrice;
        existingItem.TotalPrice = request.TotalPrice;

        // Si se proporciona IsDeductible en la request, usarlo (edición manual del usuario)
        // Si no, re-validar según políticas
        bool manualOverride = request.IsDeductible.HasValue;

        // Re-validar según políticas con los nuevos valores
        var category = existingItem.Product?.DefaultCategory;
        if (category != null && !manualOverride)
        {
            var isDeductible = category.IsDeductible ?? false;
            var validationMessages = new List<string>();
            var policyValidation = "Pending";

            // Verificar políticas activas
            var activePolicy = category.BusinessPolicies
                .Where(p => p.IsActive == true &&
                           p.EffectiveDate <= DateOnly.FromDateTime(DateTime.Now) &&
                           (p.ExpirationDate == null || p.ExpirationDate >= DateOnly.FromDateTime(DateTime.Now)))
                .FirstOrDefault();

            if (activePolicy != null)
            {
                if (activePolicy.MaxDailyAmount.HasValue && request.TotalPrice > activePolicy.MaxDailyAmount.Value)
                {
                    isDeductible = false;
                    policyValidation = "Rejected";
                    validationMessages.Add($"Excede límite diario de {activePolicy.MaxDailyAmount:C}");
                }
            }

            if (category.MaxAmountAllowed.HasValue && request.TotalPrice > category.MaxAmountAllowed.Value)
            {
                isDeductible = false;
                policyValidation = "Rejected";
                validationMessages.Add($"Excede monto máximo de {category.MaxAmountAllowed:C}");
            }

            if (isDeductible && validationMessages.Count == 0)
            {
                policyValidation = "Approved";
            }

            // Actualizar validación
            existingItem.IsDeductible = isDeductible;
            existingItem.PolicyValidation = policyValidation;
            existingItem.ValidationNotes = validationMessages.Count > 0 ? string.Join("; ", validationMessages) : existingItem.ValidationNotes;
        }
        else if (manualOverride)
        {
            // Aplicar el valor manual de IsDeductible proporcionado por el usuario
            existingItem.IsDeductible = request.IsDeductible.Value;
            existingItem.PolicyValidation = "Manual"; // Indicar que fue modificado manualmente

            // Agregar nota de edición manual
            var manualNote = "Estado modificado manualmente por el administrador";
            if (string.IsNullOrEmpty(existingItem.ValidationNotes))
            {
                existingItem.ValidationNotes = manualNote;
            }
            else if (!existingItem.ValidationNotes.Contains("modificado manualmente"))
            {
                existingItem.ValidationNotes += "; " + manualNote;
            }
        }

        existingItem.ProcessedAt = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();

            // Actualizar totales del ticket
            await UpdateTicketTotals(existingItem.TicketId);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExpenseItemExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return Ok(new
        {
            message = "Item actualizado correctamente",
            isDeductible = existingItem.IsDeductible,
            validationNotes = existingItem.ValidationNotes
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpenseItem(int id)
    {
        var item = await _context.ExpenseItems.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        _context.ExpenseItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> ExpenseItemExists(int id)
    {
        return await _context.ExpenseItems.AnyAsync(i => i.ItemId == id);
    }

    // Crear expense item con validación automática
    [HttpPost("add-with-validation")]
    public async Task<ActionResult<object>> AddExpenseItemWithValidation([FromBody] AddExpenseItemRequest request)
    {
        Product? product = null;
        Category? category = null;
        var isDeductible = false;
        var validationMessages = new List<string>();
        var policyValidation = "Pending";
        string? productName = null;
        string? brand = null;
        string? categoryName = null;

        // Caso 1: Si hay ProductId, buscar el producto en la base de datos
        if (request.ProductId.HasValue)
        {
            product = await _context.Products
                .Include(p => p.DefaultCategory)
                .ThenInclude(c => c.BusinessPolicies)
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

            if (product == null)
            {
                return NotFound("Producto no encontrado");
            }

            category = product.DefaultCategory;
            productName = product.ProductName;
            brand = product.Brand;
            categoryName = category.CategoryName;
            isDeductible = category.IsDeductible ?? false;

            // Verificar políticas activas
            var activePolicy = category.BusinessPolicies
                .Where(p => p.IsActive == true &&
                           p.EffectiveDate <= DateOnly.FromDateTime(DateTime.Now) &&
                           (p.ExpirationDate == null || p.ExpirationDate >= DateOnly.FromDateTime(DateTime.Now)))
                .FirstOrDefault();

            if (activePolicy != null)
            {
                if (activePolicy.MaxDailyAmount.HasValue && request.TotalPrice > activePolicy.MaxDailyAmount.Value)
                {
                    isDeductible = false;
                    policyValidation = "Rejected";
                    validationMessages.Add($"Excede límite diario de {activePolicy.MaxDailyAmount:C}");
                }
            }

            if (category.MaxAmountAllowed.HasValue && request.TotalPrice > category.MaxAmountAllowed.Value)
            {
                isDeductible = false;
                policyValidation = "Rejected";
                validationMessages.Add($"Excede monto máximo de {category.MaxAmountAllowed:C}");
            }

            if (isDeductible && validationMessages.Count == 0)
            {
                policyValidation = "Approved";
            }
        }
        // Caso 2: Si NO hay ProductId, es un producto analizado por IA sin match en BD
        else
        {
            // Usar la información proporcionada en la request (del análisis de Claude AI)
            isDeductible = request.IsDeductible ?? false;
            policyValidation = "Pending"; // Requerirá revisión manual
            productName = request.ItemDescription;

            // Si se proporciona CategoryId, obtener la categoría
            if (request.CategoryId.HasValue)
            {
                category = await _context.Categories.FindAsync(request.CategoryId.Value);
                if (category != null)
                {
                    categoryName = category.CategoryName;
                }
            }

            // Agregar nota de que fue analizado por IA
            var aiNote = "Producto analizado por IA - Sin match en base de datos";
            if (!string.IsNullOrEmpty(request.ValidationNotes))
            {
                validationMessages.Add(request.ValidationNotes);
            }
            validationMessages.Add(aiNote);
        }

        // Crear el expense item
        var expenseItem = new ExpenseItem
        {
            TicketId = request.TicketId,
            ItemDescription = request.ItemDescription,
            OriginalDescription = request.OriginalDescription,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalPrice = request.TotalPrice,
            CategoryId = category?.CategoryId,
            ProductId = product?.ProductId,
            IsDeductible = isDeductible,
            PolicyValidation = policyValidation,
            ValidationNotes = validationMessages.Count > 0 ? string.Join("; ", validationMessages) : null,
            ProcessedAt = DateTime.Now
        };

        _context.ExpenseItems.Add(expenseItem);
        await _context.SaveChangesAsync();

        // Actualizar totales del ticket
        await UpdateTicketTotals(request.TicketId);

        return Ok(new
        {
            ItemId = expenseItem.ItemId,
            ProductName = productName,
            Brand = brand,
            CategoryName = categoryName,
            IsDeductible = isDeductible,
            PolicyValidation = policyValidation,
            ValidationMessages = validationMessages,
            Status = policyValidation
        });
    }

    private async Task UpdateTicketTotals(int? ticketId)
    {
        if (!ticketId.HasValue) return;

        var ticket = await _context.ExpenseTickets
            .Include(t => t.ExpenseItems)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);

        if (ticket == null) return;

        ticket.DeductibleAmount = ticket.ExpenseItems
            .Where(i => i.IsDeductible == true)
            .Sum(i => i.TotalPrice);

        ticket.NonDeductibleAmount = ticket.ExpenseItems
            .Where(i => i.IsDeductible == false)
            .Sum(i => i.TotalPrice);

        ticket.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }
}

public class AddExpenseItemRequest
{
    public int TicketId { get; set; }
    public int? ProductId { get; set; } // Ahora es opcional para productos analizados por IA
    public string ItemDescription { get; set; } = null!;
    public string? OriginalDescription { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    // Campos adicionales para productos analizados por IA sin match en BD
    public int? CategoryId { get; set; }
    public bool? IsDeductible { get; set; }
    public string? ValidationNotes { get; set; }
}

public class UpdateExpenseItemRequest
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool? IsDeductible { get; set; } // Permitir editar manualmente el estado
}
