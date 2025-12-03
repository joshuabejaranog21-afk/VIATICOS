using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class BusinessPolicy
{
    public int PolicyId { get; set; }

    public string PolicyCode { get; set; } = null!;

    public string PolicyName { get; set; } = null!;

    public int? CategoryId { get; set; }

    public decimal? MaxDailyAmount { get; set; }

    public decimal? MaxMonthlyAmount { get; set; }

    public bool? RequiresReceipt { get; set; }

    public bool? RequiresManagerApproval { get; set; }

    public decimal? MinApprovalAmount { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }
}
