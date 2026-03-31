using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DeductionController : ControllerBase
{
    private readonly AppDbContext _context;
    public DeductionController(AppDbContext context) => _context = context;

    // GET: api/deduction
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Deductions.Include(d => d.Department).Include(d => d.Employee).ToListAsync());

    // GET: api/deduction/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var d = await _context.Deductions.FindAsync(id);
        return d == null ? NotFound() : Ok(d);
    }

    // POST: api/deduction
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Deduction deduction)
    {
        if (!await ValidateScope(deduction.Scope, deduction.DepartmentId, deduction.EmployeeId))
            return BadRequest(new { message = "Invalid scope or missing target reference." });

        NullifyUnusedFKs(deduction);
        deduction.CreatedAt = DateTime.UtcNow;
        _context.Deductions.Add(deduction);
        await _context.SaveChangesAsync();
        return Ok(deduction);
    }

    // PUT: api/deduction/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Deduction input)
    {
        var deduction = await _context.Deductions.FindAsync(id);
        if (deduction == null) return NotFound();

        if (!await ValidateScope(input.Scope, input.DepartmentId, input.EmployeeId))
            return BadRequest(new { message = "Invalid scope or missing target reference." });

        deduction.Name         = input.Name;
        deduction.Amount       = input.Amount;
        deduction.Scope        = input.Scope;
        deduction.DepartmentId = input.DepartmentId;
        deduction.EmployeeId   = input.EmployeeId;
        NullifyUnusedFKs(deduction);

        await _context.SaveChangesAsync();
        return Ok(deduction);
    }

    // DELETE: api/deduction/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await _context.Deductions.FindAsync(id);
        if (d == null) return NotFound();
        _context.Deductions.Remove(d);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Deduction deleted." });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<bool> ValidateScope(DeductionScope scope, int? deptId, int? empId)
    {
        return scope switch
        {
            DeductionScope.Global     => true,
            DeductionScope.Department => deptId.HasValue && await _context.Departments.AnyAsync(d => d.Id == deptId),
            DeductionScope.Individual => empId.HasValue  && await _context.Employees.AnyAsync(e => e.Id == empId),
            _                         => false
        };
    }

    private static void NullifyUnusedFKs(Deduction d)
    {
        if (d.Scope != DeductionScope.Department) d.DepartmentId = null;
        if (d.Scope != DeductionScope.Individual) d.EmployeeId   = null;
    }
}
