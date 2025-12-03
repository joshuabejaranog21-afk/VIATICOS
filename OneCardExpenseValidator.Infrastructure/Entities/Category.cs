using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeductible { get; set; }

    public bool? RequiresApproval { get; set; }

    public decimal? MaxAmountAllowed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BusinessPolicy> BusinessPolicies { get; set; } = new List<BusinessPolicy>();

    public virtual ICollection<CategorizationLog> CategorizationLogCorrectCategories { get; set; } = new List<CategorizationLog>();

    public virtual ICollection<CategorizationLog> CategorizationLogPredictedCategories { get; set; } = new List<CategorizationLog>();

    public virtual ICollection<CategoryKeyword> CategoryKeywords { get; set; } = new List<CategoryKeyword>();

    public virtual ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
