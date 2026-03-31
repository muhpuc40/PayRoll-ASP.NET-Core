using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AllowanceController : ControllerBase
{
    private readonly AppDbContext _context;
    public AllowanceController(AppDbContext context) => _context = context;

    // GET: api/allowance
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Allowances.Include(a => a.Department).Include(a => a.Employee).ToListAsync());

    // GET: api/allowance/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await _context.Allowances.FindAsync(id);
        return a == null ? NotFound() : Ok(a);
    }

    // POST: api/allowance
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Allowance allowance)
    {
        if (!await ValidateScope(allowance.Scope, allowance.DepartmentId, allowance.EmployeeId))
            return BadRequest(new { message = "Invalid scope or missing target reference." });

        NullifyUnusedFKs(allowance);
        allowance.CreatedAt = DateTime.UtcNow;
        _context.Allowances.Add(allowance);
        await _context.SaveChangesAsync();
        return Ok(allowance);
    }

    // PUT: api/allowance/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Allowance input)
    {
        var allowance = await _context.Allowances.FindAsync(id);
        if (allowance == null) return NotFound();

        if (!await ValidateScope(input.Scope, input.DepartmentId, input.EmployeeId))
            return BadRequest(new { message = "Invalid scope or missing target reference." });

        allowance.Name         = input.Name;
        allowance.Amount       = input.Amount;
        allowance.Scope        = input.Scope;
        allowance.DepartmentId = input.DepartmentId;
        allowance.EmployeeId   = input.EmployeeId;
        NullifyUnusedFKs(allowance);

        await _context.SaveChangesAsync();
        return Ok(allowance);
    }

    // DELETE: api/allowance/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _context.Allowances.FindAsync(id);
        if (a == null) return NotFound();
        _context.Allowances.Remove(a);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Allowance deleted." });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<bool> ValidateScope(AllowanceScope scope, int? deptId, int? empId)
    {
        return scope switch
        {
            AllowanceScope.Global     => true,
            AllowanceScope.Department => deptId.HasValue && await _context.Departments.AnyAsync(d => d.Id == deptId),
            AllowanceScope.Individual => empId.HasValue  && await _context.Employees.AnyAsync(e => e.Id == empId),
            _                         => false
        };
    }

    private static void NullifyUnusedFKs(Allowance a)
    {
        if (a.Scope != AllowanceScope.Department) a.DepartmentId = null;
        if (a.Scope != AllowanceScope.Individual) a.EmployeeId   = null;
    }
}
