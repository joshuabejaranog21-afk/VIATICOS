using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string? Gtin { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Brand { get; set; }

    public int DefaultCategoryId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category DefaultCategory { get; set; } = null!;

    public virtual ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();

    public virtual ICollection<ProductAlias> ProductAliases { get; set; } = new List<ProductAlias>();
}
