using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class AuditLog
{
    public int AuditId { get; set; }

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public string? Action { get; set; }

    public int? UserId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime? ActionDate { get; set; }
}
