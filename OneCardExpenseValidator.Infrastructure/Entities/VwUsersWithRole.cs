using System;
using System.Collections.Generic;

namespace OneCardExpenseValidator.Infrastructure.Entities;

public partial class VwUsersWithRole
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? LastLogin { get; set; }

    public string? Roles { get; set; }

    public string? EmployeeCode { get; set; }

    public int? DepartmentId { get; set; }
}
