using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class ExpenseItem
{
    public int ItemId { get; set; }

    public int? TicketId { get; set; }

    public string ItemDescription { get; set; } = null!;

    public string? OriginalDescription { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public int? CategoryId { get; set; }

    public double? SemanticScore { get; set; }

    public bool? IsDeductible { get; set; }

    public string? PolicyValidation { get; set; }

    public string? ValidationNotes { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int? ProductId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Product? Product { get; set; }

    public virtual ExpenseTicket? Ticket { get; set; }
}
