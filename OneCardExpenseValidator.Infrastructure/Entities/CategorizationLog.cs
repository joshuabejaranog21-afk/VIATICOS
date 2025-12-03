using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class CategorizationLog
{
    public int LogId { get; set; }

    public string? ItemDescription { get; set; }

    public int? PredictedCategoryId { get; set; }

    public double? ConfidenceScore { get; set; }

    public string? AlgorithmUsed { get; set; }

    public bool? WasCorrect { get; set; }

    public int? CorrectCategoryId { get; set; }

    public int? ProcessingTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? CorrectCategory { get; set; }

    public virtual Category? PredictedCategory { get; set; }
}
