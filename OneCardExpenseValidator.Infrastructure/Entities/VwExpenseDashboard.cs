using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class VwExpenseDashboard
{
    public string DepartmentName { get; set; } = null!;

    public int? ActiveEmployees { get; set; }

    public int? TotalTickets { get; set; }

    public decimal? TotalExpenses { get; set; }

    public decimal? TotalDeductible { get; set; }

    public int? ApprovedCount { get; set; }

    public int? RejectedCount { get; set; }

    public int? AvgProcessingHours { get; set; }
}
