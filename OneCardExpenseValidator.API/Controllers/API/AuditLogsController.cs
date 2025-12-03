using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditLogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs([FromQuery] int limit = 100)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.ActionDate)
            .Take(limit)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuditLog>> GetAuditLog(int id)
    {
        var auditLog = await _context.AuditLogs.FindAsync(id);

        if (auditLog == null)
        {
            return NotFound();
        }

        return auditLog;
    }

    [HttpGet("table/{tableName}")]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetLogsByTable(string tableName)
    {
        return await _context.AuditLogs
            .Where(a => a.TableName == tableName)
            .OrderByDescending(a => a.ActionDate)
            .ToListAsync();
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetLogsByUser(int userId)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.ActionDate)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<AuditLog>> CreateAuditLog(AuditLog auditLog)
    {
        auditLog.ActionDate = DateTime.Now;

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAuditLog), new { id = auditLog.AuditId }, auditLog);
    }
}
