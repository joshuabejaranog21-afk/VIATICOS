using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class MonthlyExpenseReport
{
    public int ReportId { get; set; }

    public int? EmployeeId { get; set; }

    public int? ReportMonth { get; set; }

    public int? ReportYear { get; set; }

    public decimal? TotalExpenses { get; set; }

    public decimal? TotalDeductible { get; set; }

    public decimal? TotalNonDeductible { get; set; }

    public decimal? TotalApproved { get; set; }

    public decimal? TotalRejected { get; set; }

    public DateTime? GeneratedDate { get; set; }

    public virtual Employee? Employee { get; set; }
}
