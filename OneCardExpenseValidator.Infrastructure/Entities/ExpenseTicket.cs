using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class ExpenseTicket
{
    public int TicketId { get; set; }

    public string? TicketNumber { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateOnly TicketDate { get; set; }

    public string? Vendor { get; set; }

    public string? TicketImagePath { get; set; }

    public string? OcrextractedText { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DeductibleAmount { get; set; }

    public decimal? NonDeductibleAmount { get; set; }

    public string? ValidationStatus { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public string? RejectionReason { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual Employee? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();
}
