using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class VwPendingExpense
{
    public int TicketId { get; set; }

    public string? TicketNumber { get; set; }

    public string EmployeeName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public DateOnly TicketDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? ValidationStatus { get; set; }

    public int? DaysPending { get; set; }
}
