using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoleUsers)
                .ThenInclude(ur => ur.Role)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                u.LastLogin,
                u.EmployeeId,
                u.CreatedAt,
                Employee = u.Employee != null ? new
                {
                    u.Employee.EmployeeCode,
                    u.Employee.Position,
                    DepartmentName = u.Employee.Department != null ? u.Employee.Department.DepartmentName : null
                } : null,
                Roles = u.UserRoleUsers.Select(ur => new
                {
                    ur.Role.RoleId,
                    ur.Role.RoleName
                }).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoleUsers)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.UserId == id)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                u.LastLogin,
                u.EmployeeId,
                u.CreatedAt,
                u.UpdatedAt,
                Employee = u.Employee != null ? new
                {
                    u.Employee.EmployeeCode,
                    u.Employee.FirstName,
                    u.Employee.LastName,
                    u.Employee.Position,
                    DepartmentName = u.Employee.Department != null ? u.Employee.Department.DepartmentName : null
                } : null,
                Roles = u.UserRoleUsers.Select(ur => new
                {
                    ur.UserRoleId,
                    ur.Role.RoleId,
                    ur.Role.RoleName,
                    ur.AssignedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<object>> GetUserByUsername(string username)
    {
        var user = await _context.Users
            .Include(u => u.UserRoleUsers)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.Username == username)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                Roles = u.UserRoleUsers.Select(ur => ur.Role.RoleName).ToList()
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Email = dto.Email ?? user.Email;
        user.FirstName = dto.FirstName ?? user.FirstName;
        user.LastName = dto.LastName ?? user.LastName;
        user.IsActive = dto.IsActive ?? user.IsActive;
        user.UpdatedAt = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await UserExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> UserExists(int id)
    {
        return await _context.Users.AnyAsync(u => u.UserId == id);
    }
}

// DTO para actualizar usuario
public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? IsActive { get; set; }
}
