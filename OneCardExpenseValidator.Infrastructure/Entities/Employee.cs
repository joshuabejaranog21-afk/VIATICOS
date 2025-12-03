using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? DepartmentId { get; set; }

    public string? Position { get; set; }

    public decimal? DailyExpenseLimit { get; set; }

    public decimal? MonthlyExpenseLimit { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<ExpenseTicket> ExpenseTicketApprovedByNavigations { get; set; } = new List<ExpenseTicket>();

    public virtual ICollection<ExpenseTicket> ExpenseTicketEmployees { get; set; } = new List<ExpenseTicket>();

    public virtual ICollection<MonthlyExpenseReport> MonthlyExpenseReports { get; set; } = new List<MonthlyExpenseReport>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
