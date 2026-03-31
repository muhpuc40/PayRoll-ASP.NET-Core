using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PayrollAPI.Data;
using PayrollAPI.Models;
using PayrollAPI.Services;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayslipController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PayrollCalculationService _payrollService;

    public PayslipController(AppDbContext context, PayrollCalculationService payrollService)
    {
        _context        = context;
        _payrollService = payrollService;
    }

    // ── Admin: Generate payslip ───────────────────────────────────────────────

    // POST: api/payslip/generate
    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Generate([FromBody] GeneratePayslipRequest req)
    {
        try
        {
            var payslip = await _payrollService.GeneratePayslipAsync(
                req.EmployeeId, req.Month, req.Year);

            return Ok(await BuildPayslipResponse(payslip.Id));
        }
        catch (KeyNotFoundException ex)  { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // POST: api/payslip/generate-all?month=3&year=2025
    // Generates payslips for ALL active employees in one call
    [HttpPost("generate-all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateAll([FromQuery] int month, [FromQuery] int year)
    {
        var employeeIds = await _context.Employees
            .Where(e => e.IsActive)
            .Select(e => e.Id)
            .ToListAsync();

        var results = new List<object>();

        foreach (var empId in employeeIds)
        {
            try
            {
                var payslip = await _payrollService.GeneratePayslipAsync(empId, month, year);
                results.Add(new { EmployeeId = empId, PayslipId = payslip.Id, Status = "Generated" });
            }
            catch (Exception ex)
            {
                results.Add(new { EmployeeId = empId, Status = "Skipped", Reason = ex.Message });
            }
        }

        return Ok(results);
    }

    // ── Admin: List all payslips ──────────────────────────────────────────────

    // GET: api/payslip?month=3&year=2025
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year)
    {
        var query = _context.Payslips
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .AsQueryable();

        if (month.HasValue) query = query.Where(p => p.Month == month.Value);
        if (year.HasValue)  query = query.Where(p => p.Year  == year.Value);

        var payslips = await query
            .OrderBy(p => p.Year).ThenBy(p => p.Month)
            .ThenBy(p => p.Employee.LastName)
            .Select(p => new
            {
                p.Id, p.Month, p.Year,
                p.BaseSalary, p.TotalAllowances, p.TotalBonuses,
                p.TotalDeductions, p.AttendancePenalty, p.NetPayable,
                p.Status, p.GeneratedAt,
                Employee = new
                {
                    p.Employee.Id,
                    FullName   = p.Employee.FirstName + " " + p.Employee.LastName,
                    p.Employee.Position,
                    Department = p.Employee.Department.Name
                }
            })
            .ToListAsync();

        return Ok(payslips);
    }

    // ── Admin Dashboard Summary ───────────────────────────────────────────────

    // GET: api/payslip/summary?month=3&year=2025
    [HttpGet("summary")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSummary([FromQuery] int month, [FromQuery] int year)
    {
        var payslips = await _context.Payslips
            .Where(p => p.Month == month && p.Year == year)
            .ToListAsync();

        return Ok(new
        {
            Month          = month,
            Year           = year,
            EmployeeCount  = payslips.Count,
            TotalBaseSalary    = payslips.Sum(p => p.BaseSalary),
            TotalAllowances    = payslips.Sum(p => p.TotalAllowances),
            TotalBonuses       = payslips.Sum(p => p.TotalBonuses),
            TotalDeductions    = payslips.Sum(p => p.TotalDeductions),
            TotalAttendancePenalty = payslips.Sum(p => p.AttendancePenalty),
            TotalNetPayable    = payslips.Sum(p => p.NetPayable)
        });
    }

    // ── Get single payslip (Admin: any; Employee: own only) ───────────────────

    // GET: api/payslip/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payslip == null) return NotFound(new { message = "Payslip not found." });

        // Employees can only view their own payslips
        if (!User.IsInRole("Admin"))
        {
            var userId    = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ownEmpId  = await _context.Employees
                .Where(e => e.UserId == userId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (payslip.EmployeeId != ownEmpId)
                return Forbid();
        }

        return Ok(BuildFormattedPayslip(payslip));
    }

    // GET: api/payslip/employee/5?month=3&year=2025
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(
        int employeeId, [FromQuery] int? month, [FromQuery] int? year)
    {
        // Employees can only access their own
        if (!User.IsInRole("Admin"))
        {
            var userId   = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ownEmpId = await _context.Employees
                .Where(e => e.UserId == userId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (employeeId != ownEmpId) return Forbid();
        }

        var query = _context.Payslips
            .Include(p => p.LineItems)
            .Where(p => p.EmployeeId == employeeId);

        if (month.HasValue) query = query.Where(p => p.Month == month.Value);
        if (year.HasValue)  query = query.Where(p => p.Year  == year.Value);

        var payslips = await query
            .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
            .ToListAsync();

        return Ok(payslips.Select(BuildFormattedPayslip));
    }

    // ── Status transitions (Admin only) ──────────────────────────────────────

    // PUT: api/payslip/5/approve
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var payslip = await _context.Payslips.FindAsync(id);
        if (payslip == null) return NotFound();
        if (payslip.Status != PayslipStatus.Draft)
            return BadRequest(new { message = "Only Draft payslips can be approved." });

        payslip.Status = PayslipStatus.Approved;
        await _context.SaveChangesAsync();
        return Ok(new { payslip.Id, payslip.Status });
    }

    // PUT: api/payslip/5/pay
    [HttpPut("{id}/pay")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var payslip = await _context.Payslips.FindAsync(id);
        if (payslip == null) return NotFound();
        if (payslip.Status != PayslipStatus.Approved)
            return BadRequest(new { message = "Only Approved payslips can be marked Paid." });

        payslip.Status = PayslipStatus.Paid;
        await _context.SaveChangesAsync();
        return Ok(new { payslip.Id, payslip.Status });
    }

    // DELETE: api/payslip/5  (Draft only)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var payslip = await _context.Payslips.FindAsync(id);
        if (payslip == null) return NotFound();
        if (payslip.Status != PayslipStatus.Draft)
            return BadRequest(new { message = "Only Draft payslips can be deleted." });

        _context.Payslips.Remove(payslip);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Payslip deleted." });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<object> BuildPayslipResponse(int payslipId)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.LineItems)
            .FirstAsync(p => p.Id == payslipId);

        return BuildFormattedPayslip(payslip);
    }

    private static object BuildFormattedPayslip(Payslip p)
    {
        var allowances = p.LineItems.Where(l => l.Category == "Allowance").ToList();
        var bonuses    = p.LineItems.Where(l => l.Category == "Bonus").ToList();
        var deductions = p.LineItems.Where(l => l.Category == "Deduction").ToList();
        var penalties  = p.LineItems.Where(l => l.Category == "AttendancePenalty").ToList();

        return new
        {
            p.Id,
            p.Month,
            p.Year,
            p.Status,
            p.GeneratedAt,
            Employee = p.Employee == null ? null : new
            {
                p.Employee.Id,
                FullName   = $"{p.Employee.FirstName} {p.Employee.LastName}",
                p.Employee.Position,
                Department = p.Employee.Department?.Name
            },

            // ── Payslip breakdown ───────────────────────
            BaseSalary = p.BaseSalary,

            Allowances = allowances.Select(l => new { l.Label, l.Amount }),
            TotalAllowances = p.TotalAllowances,

            Bonuses = bonuses.Select(l => new { l.Label, l.Amount }),
            TotalBonuses = p.TotalBonuses,

            Deductions = deductions.Select(l => new { l.Label, Amount = -l.Amount }),
            TotalDeductions = p.TotalDeductions,

            AttendancePenalties = penalties.Select(l => new { l.Label, Amount = -l.Amount }),
            TotalAttendancePenalty = p.AttendancePenalty,

            // ── Totals ───────────────────────────────────
            GrossEarnings = p.BaseSalary + p.TotalAllowances + p.TotalBonuses,
            TotalReductions = p.TotalDeductions + p.AttendancePenalty,
            NetPayable = p.NetPayable
        };
    }
}

public record GeneratePayslipRequest(int EmployeeId, int Month, int Year);
