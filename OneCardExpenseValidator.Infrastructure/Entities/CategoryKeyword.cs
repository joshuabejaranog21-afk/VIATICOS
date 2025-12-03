using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class CategoryKeyword
{
    public int KeywordId { get; set; }

    public int? CategoryId { get; set; }

    public string Keyword { get; set; } = null!;

    public double? Weight { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }
}
