using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class ProductAlias
{
    public int AliasId { get; set; }

    public int ProductId { get; set; }

    public string Alias { get; set; } = null!;

    public string? Source { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
