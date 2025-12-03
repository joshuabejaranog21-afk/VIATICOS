using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategorizationLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategorizationLogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategorizationLog>>> GetCategorizationLogs()
    {
        return await _context.CategorizationLogs
            .Include(l => l.PredictedCategory)
            .Include(l => l.CorrectCategory)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategorizationLog>> GetCategorizationLog(int id)
    {
        var log = await _context.CategorizationLogs
            .Include(l => l.PredictedCategory)
            .Include(l => l.CorrectCategory)
            .FirstOrDefaultAsync(l => l.LogId == id);

        if (log == null)
        {
            return NotFound();
        }

        return log;
    }

    [HttpGet("incorrect")]
    public async Task<ActionResult<IEnumerable<CategorizationLog>>> GetIncorrectPredictions()
    {
        return await _context.CategorizationLogs
            .Include(l => l.PredictedCategory)
            .Include(l => l.CorrectCategory)
            .Where(l => l.WasCorrect == false)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<CategorizationLog>> CreateCategorizationLog(CategorizationLog log)
    {
        log.CreatedAt = DateTime.Now;

        _context.CategorizationLogs.Add(log);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategorizationLog), new { id = log.LogId }, log);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategorizationLog(int id, CategorizationLog log)
    {
        if (id != log.LogId)
        {
            return BadRequest();
        }

        _context.Entry(log).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CategorizationLogExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategorizationLog(int id)
    {
        var log = await _context.CategorizationLogs.FindAsync(id);
        if (log == null)
        {
            return NotFound();
        }

        _context.CategorizationLogs.Remove(log);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> CategorizationLogExists(int id)
    {
        return await _context.CategorizationLogs.AnyAsync(l => l.LogId == id);
    }
}
