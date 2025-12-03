using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessPoliciesController : ControllerBase
{
    private readonly AppDbContext _context;

    public BusinessPoliciesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessPolicy>>> GetBusinessPolicies()
    {
        return await _context.BusinessPolicies
            .Include(p => p.Category)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BusinessPolicy>> GetBusinessPolicy(int id)
    {
        var policy = await _context.BusinessPolicies
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.PolicyId == id);

        if (policy == null)
        {
            return NotFound();
        }

        return policy;
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<BusinessPolicy>>> GetActivePolicies()
    {
        return await _context.BusinessPolicies
            .Include(p => p.Category)
            .Where(p => p.IsActive == true)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<BusinessPolicy>> CreateBusinessPolicy(BusinessPolicy policy)
    {
        policy.CreatedAt = DateTime.Now;

        _context.BusinessPolicies.Add(policy);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBusinessPolicy), new { id = policy.PolicyId }, policy);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBusinessPolicy(int id, BusinessPolicy policy)
    {
        if (id != policy.PolicyId)
        {
            return BadRequest();
        }

        _context.Entry(policy).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await BusinessPolicyExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBusinessPolicy(int id)
    {
        var policy = await _context.BusinessPolicies.FindAsync(id);
        if (policy == null)
        {
            return NotFound();
        }

        _context.BusinessPolicies.Remove(policy);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> BusinessPolicyExists(int id)
    {
        return await _context.BusinessPolicies.AnyAsync(p => p.PolicyId == id);
    }
}
