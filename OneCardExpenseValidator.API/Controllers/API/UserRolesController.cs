using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserRolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserRolesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUserRoles()
    {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Include(ur => ur.AssignedByNavigation)
            .Select(ur => new
            {
                ur.UserRoleId,
                ur.UserId,
                ur.RoleId,
                ur.AssignedAt,
                User = new
                {
                    ur.User.Username,
                    ur.User.FirstName,
                    ur.User.LastName,
                    ur.User.Email
                },
                Role = new
                {
                    ur.Role.RoleName,
                    ur.Role.Description
                },
                AssignedBy = ur.AssignedByNavigation != null ? new
                {
                    ur.AssignedByNavigation.Username,
                    ur.AssignedByNavigation.FirstName,
                    ur.AssignedByNavigation.LastName
                } : null
            })
            .ToListAsync();

        return Ok(userRoles);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetRolesByUser(int userId)
    {
        var roles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .Select(ur => new
            {
                ur.UserRoleId,
                ur.RoleId,
                ur.Role.RoleName,
                ur.Role.Description,
                ur.AssignedAt
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("role/{roleId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetUsersByRole(int roleId)
    {
        var users = await _context.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => new
            {
                ur.UserRoleId,
                ur.UserId,
                ur.User.Username,
                ur.User.FirstName,
                ur.User.LastName,
                ur.User.Email,
                ur.AssignedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserRole>> AssignRoleToUser([FromBody] AssignRoleDto dto)
    {
        // Verificar si el usuario existe
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        // Verificar si el rol existe
        var role = await _context.Roles.FindAsync(dto.RoleId);
        if (role == null)
        {
            return NotFound(new { message = "Rol no encontrado" });
        }

        // Verificar si ya tiene ese rol asignado
        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

        if (existingUserRole != null)
        {
            return BadRequest(new { message = "El usuario ya tiene ese rol asignado" });
        }

        var userRole = new UserRole
        {
            UserId = dto.UserId,
            RoleId = dto.RoleId,
            AssignedAt = DateTime.Now,
            AssignedBy = dto.AssignedBy
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRolesByUser), new { userId = userRole.UserId }, userRole);
    }

    [HttpDelete("{userRoleId}")]
    public async Task<IActionResult> RemoveRoleFromUser(int userRoleId)
    {
        var userRole = await _context.UserRoles.FindAsync(userRoleId);
        if (userRole == null)
        {
            return NotFound();
        }

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("user/{userId}/role/{roleId}")]
    public async Task<IActionResult> RemoveRoleFromUserByIds(int userId, int roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null)
        {
            return NotFound();
        }

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTO para asignar rol
public class AssignRoleDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public int? AssignedBy { get; set; }
}
