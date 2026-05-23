using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Models;
using PayrollAPI.Models.DTOs;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;
    public AttendanceController(AppDbContext context) => _context = context;

    // GET: api/attendance/{employeeId}?month=3&year=2025
    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetByEmployee(
        int employeeId, [FromQuery] int? month, [FromQuery] int? year)
    {
        var query = _context.Attendances.Where(a => a.EmployeeId == employeeId);
        if (month.HasValue) query = query.Where(a => a.Date.Month == month.Value);
        if (year.HasValue) query = query.Where(a => a.Date.Year == year.Value);

        return Ok(await query.OrderBy(a => a.Date).ToListAsync());
    }

    // GET: api/attendance/summary/{employeeId}?month=3&year=2025
    [HttpGet("summary/{employeeId}")]
    public async Task<IActionResult> GetSummary(
        int employeeId, [FromQuery] int month, [FromQuery] int year)
    {
        var records = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId
                     && a.Date.Month == month && a.Date.Year == year)
            .ToListAsync();

        return Ok(new
        {
            EmployeeId = employeeId,
            Month = month,
            Year = year,
            TotalRecorded = records.Count,
            Present = records.Count(a => a.Status == AttendanceStatus.Present),
            Absent = records.Count(a => a.Status == AttendanceStatus.Absent),
            Late = records.Count(a => a.Status == AttendanceStatus.Late)
        });
    }

    // POST: api/attendance  (Admin only)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add([FromBody] AttendanceDto dto)
    {
        if (!await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId))
            return NotFound(new { message = "Employee not found." });

        bool exists = await _context.Attendances.AnyAsync(a =>
            a.EmployeeId == dto.EmployeeId && a.Date.Date == dto.Date.Date);

        if (exists)
            return BadRequest(new { message = "Attendance already recorded for this date." });

        var attendance = new Attendance
        {
            EmployeeId = dto.EmployeeId,
            Date = dto.Date.Date,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return Ok(attendance);
    }

    // POST: api/attendance/bulk  (Admin only)
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkAdd([FromBody] List<AttendanceDto> dtos)
    {
        var records = new List<Attendance>();

        foreach (var dto in dtos)
        {
            bool exists = await _context.Attendances.AnyAsync(a =>
                a.EmployeeId == dto.EmployeeId && a.Date.Date == dto.Date.Date);

            if (exists)
                return BadRequest(new { message = $"Attendance already recorded for Employee {dto.EmployeeId} on {dto.Date:yyyy-MM-dd}." });

            records.Add(new Attendance
            {
                EmployeeId = dto.EmployeeId,
                Date = dto.Date.Date,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Attendances.AddRange(records);
        await _context.SaveChangesAsync();
        return Ok(new { message = $"{records.Count} records added." });
    }

    // PUT: api/attendance/5  (Admin only)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] AttendanceDto dto)
    {
        var record = await _context.Attendances.FindAsync(id);
        if (record == null) return NotFound();

        record.Date = dto.Date.Date;
        record.Status = dto.Status;
        await _context.SaveChangesAsync();
        return Ok(record);
    }

    // DELETE: api/attendance/5  (Admin only)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _context.Attendances.FindAsync(id);
        if (record == null) return NotFound();

        _context.Attendances.Remove(record);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Attendance record deleted." });
    }
}