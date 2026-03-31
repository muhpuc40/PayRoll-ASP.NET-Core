using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BonusController : ControllerBase
{
    private readonly AppDbContext _context;
    public BonusController(AppDbContext context) => _context = context;

    // GET: api/bonus
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Bonuses.Include(b => b.Department).Include(b => b.Employee).ToListAsync());

    // GET: api/bonus/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var b = await _context.Bonuses.FindAsync(id);
        return b == null ? NotFound() : Ok(b);
    }

    // POST: api/bonus
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Bonus bonus)
    {
        if (!await ValidateScope(bonus.Scope, bonus.DepartmentId, bonus.EmployeeId))
            return BadRequest(new { message = "Invalid scope or missing target reference." });

        NullifyUnusedFKs(bonus);
        bonus.CreatedAt = DateTime.UtcNow;
        _context.Bonuses.Add(bonus);
        await _context.SaveChangesAsync();
        return Ok(bonus);
    }

    // PUT: api/bonus/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Bonus input)
    {
        var bonus = await _context.Bonuses.FindAsync(id);
        if (bonus == null) return NotFound();

        if (!await ValidateScope(input.Scope, input.DepartmentId, input.EmployeeId))
            return BadRequest(new { message = "Invalid scope or missing target reference." });

        bonus.Name         = input.Name;
        bonus.Type         = input.Type;
        bonus.Value        = input.Value;
        bonus.Scope        = input.Scope;
        bonus.DepartmentId = input.DepartmentId;
        bonus.EmployeeId   = input.EmployeeId;
        NullifyUnusedFKs(bonus);

        await _context.SaveChangesAsync();
        return Ok(bonus);
    }

    // DELETE: api/bonus/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var b = await _context.Bonuses.FindAsync(id);
        if (b == null) return NotFound();
        _context.Bonuses.Remove(b);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Bonus deleted." });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<bool> ValidateScope(BonusScope scope, int? deptId, int? empId)
    {
        return scope switch
        {
            BonusScope.Global     => true,
            BonusScope.Department => deptId.HasValue && await _context.Departments.AnyAsync(d => d.Id == deptId),
            BonusScope.Individual => empId.HasValue  && await _context.Employees.AnyAsync(e => e.Id == empId),
            _                     => false
        };
    }

    private static void NullifyUnusedFKs(Bonus b)
    {
        if (b.Scope != BonusScope.Department) b.DepartmentId = null;
        if (b.Scope != BonusScope.Individual) b.EmployeeId   = null;
    }
}
