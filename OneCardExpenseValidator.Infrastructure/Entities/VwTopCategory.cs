using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class VwTopCategory
{
    public string CategoryName { get; set; } = null!;

    public int? ItemCount { get; set; }

    public decimal? TotalSpent { get; set; }

    public double? AvgConfidence { get; set; }

    public bool? IsDeductible { get; set; }
}
