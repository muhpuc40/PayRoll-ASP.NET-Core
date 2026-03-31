using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Controllers;  // for AuthController.GenerateRandomPassword
using PayrollAPI.Data;
using PayrollAPI.Models;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;
    public EmployeeController(AppDbContext context) => _context = context;

    // GET: api/employee  (Admin sees all, Employee sees own)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (User.IsInRole("Admin"))
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.User)
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    e.Id, e.FirstName, e.LastName, e.Email, e.Phone,
                    e.Position, e.HireDate, e.IsActive,
                    Department = new { e.Department.Id, e.Department.Name, e.Department.BaseSalary },
                    Username = e.User.Username
                })
                .ToListAsync();
            return Ok(employees);
        }

        // Employee: return own record only
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var emp = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.UserId == userId)
            .Select(e => new
            {
                e.Id, e.FirstName, e.LastName, e.Email, e.Phone,
                e.Position, e.HireDate,
                Department = new { e.Department.Id, e.Department.Name }
            })
            .FirstOrDefaultAsync();

        return emp == null ? NotFound() : Ok(emp);
    }

    // GET: api/employee/5
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var emp = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        return emp == null ? NotFound(new { message = "Employee not found." }) : Ok(emp);
    }

    /// <summary>
    /// Admin creates an employee account.
    /// System generates a random password returned once in the response.
    /// </summary>
    // POST: api/employee
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest req)
    {
        if (await _context.Users.AnyAsync(u => u.Username == req.Username))
            return BadRequest(new { message = "Username already exists." });

        if (await _context.Employees.AnyAsync(e => e.Email == req.Email))
            return BadRequest(new { message = "Email already in use." });

        if (!await _context.Departments.AnyAsync(d => d.Id == req.DepartmentId))
            return BadRequest(new { message = "Department not found." });

        string plainPassword = AuthController.GenerateRandomPassword();

        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
            Role = "Employee",
            MustChangePassword = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();  // get user.Id

        var employee = new Employee
        {
            UserId = user.Id,
            DepartmentId = req.DepartmentId,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email,
            Phone = req.Phone,
            Position = req.Position,
            HireDate = req.HireDate
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Employee created successfully.",
            employeeId = employee.Id,
            username = user.Username,
            generatedPassword = plainPassword   // Return ONCE — admin must share securely
        });
    }

    // PUT: api/employee/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest req)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp == null) return NotFound(new { message = "Employee not found." });

        if (!await _context.Departments.AnyAsync(d => d.Id == req.DepartmentId))
            return BadRequest(new { message = "Department not found." });

        emp.FirstName    = req.FirstName;
        emp.LastName     = req.LastName;
        emp.Email        = req.Email;
        emp.Phone        = req.Phone;
        emp.Position     = req.Position;
        emp.HireDate     = req.HireDate;
        emp.DepartmentId = req.DepartmentId;

        await _context.SaveChangesAsync();
        return Ok(emp);
    }

    // DELETE: api/employee/5  (soft delete)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp == null) return NotFound(new { message = "Employee not found." });

        emp.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Employee deactivated." });
    }
}

// ── Request records ──────────────────────────────────────────────────────────

public record CreateEmployeeRequest(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    string Position,
    int DepartmentId,
    DateTime HireDate);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Position,
    int DepartmentId,
    DateTime HireDate);
