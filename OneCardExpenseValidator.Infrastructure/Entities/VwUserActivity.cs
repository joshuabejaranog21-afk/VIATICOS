using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class VwUserActivity
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime? LastLogin { get; set; }

    public int? TotalTicketsCreated { get; set; }

    public int? TicketsApproved { get; set; }

    public int? TicketsRejected { get; set; }

    public DateTime? LastTicketSubmitted { get; set; }
}
