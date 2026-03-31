using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DepartmentController : ControllerBase
{
    private readonly AppDbContext _context;
    public DepartmentController(AppDbContext context) => _context = context;

    // GET: api/department
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Departments
            .Include(d => d.Employees.Where(e => e.IsActive))
            .ToListAsync());

    // GET: api/department/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dept = await _context.Departments
            .Include(d => d.Employees.Where(e => e.IsActive))
            .FirstOrDefaultAsync(d => d.Id == id);

        return dept == null ? NotFound(new { message = "Department not found." }) : Ok(dept);
    }

    // POST: api/department
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Department dept)
    {
        if (await _context.Departments.AnyAsync(d => d.Name == dept.Name))
            return BadRequest(new { message = "Department name already exists." });

        dept.CreatedAt = DateTime.UtcNow;
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dept.Id }, dept);
    }

    // PUT: api/department/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Department input)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) return NotFound(new { message = "Department not found." });

        dept.Name = input.Name;
        dept.BaseSalary = input.BaseSalary;
        await _context.SaveChangesAsync();
        return Ok(dept);
    }

    // DELETE: api/department/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) return NotFound(new { message = "Department not found." });

        bool hasEmployees = await _context.Employees.AnyAsync(e => e.DepartmentId == id && e.IsActive);
        if (hasEmployees)
            return BadRequest(new { message = "Cannot delete a department that still has active employees." });

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Department deleted." });
    }
}
