using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Role>> GetRole(int id)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
            .FirstOrDefaultAsync(r => r.RoleId == id);

        if (role == null)
        {
            return NotFound();
        }

        return role;
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Role>>> GetActiveRoles()
    {
        return await _context.Roles
            .Where(r => r.IsActive == true)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Role>> CreateRole(Role role)
    {
        role.CreatedAt = DateTime.Now;
        role.IsActive = true;

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, role);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, Role role)
    {
        if (id != role.RoleId)
        {
            return BadRequest();
        }

        _context.Entry(role).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await RoleExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var hasUsers = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
        if (hasUsers)
        {
            return BadRequest(new { message = "No se puede eliminar un rol que tiene usuarios asignados" });
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> RoleExists(int id)
    {
        return await _context.Roles.AnyAsync(r => r.RoleId == id);
    }
}
