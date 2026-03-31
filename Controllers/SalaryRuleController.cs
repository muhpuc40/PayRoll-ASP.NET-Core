using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SalaryRuleController : ControllerBase
{
    private readonly AppDbContext _context;
    public SalaryRuleController(AppDbContext context) => _context = context;

    // GET: api/salaryrule
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.SalaryRules.OrderBy(r => r.ConditionType).ToListAsync());

    // GET: api/salaryrule/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rule = await _context.SalaryRules.FindAsync(id);
        return rule == null ? NotFound() : Ok(rule);
    }

    // POST: api/salaryrule
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SalaryRule rule)
    {
        if (rule.Threshold < 1)
            return BadRequest(new { message = "Threshold must be at least 1." });
        if (rule.PenaltyDays <= 0)
            return BadRequest(new { message = "PenaltyDays must be greater than 0." });

        rule.CreatedAt = DateTime.UtcNow;
        _context.SalaryRules.Add(rule);
        await _context.SaveChangesAsync();
        return Ok(rule);
    }

    // PUT: api/salaryrule/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SalaryRule input)
    {
        var rule = await _context.SalaryRules.FindAsync(id);
        if (rule == null) return NotFound();

        rule.Name          = input.Name;
        rule.ConditionType = input.ConditionType;
        rule.Threshold     = input.Threshold;
        rule.PenaltyDays   = input.PenaltyDays;
        rule.IsActive      = input.IsActive;

        await _context.SaveChangesAsync();
        return Ok(rule);
    }

    // PATCH: api/salaryrule/5/toggle  — quickly enable/disable a rule
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var rule = await _context.SalaryRules.FindAsync(id);
        if (rule == null) return NotFound();

        rule.IsActive = !rule.IsActive;
        await _context.SaveChangesAsync();
        return Ok(new { rule.Id, rule.Name, rule.IsActive });
    }

    // DELETE: api/salaryrule/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var rule = await _context.SalaryRules.FindAsync(id);
        if (rule == null) return NotFound();

        _context.SalaryRules.Remove(rule);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Rule deleted." });
    }
}
