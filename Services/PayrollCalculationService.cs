using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Models;

namespace PayrollAPI.Services;

public class PayrollCalculationService
{
    private readonly AppDbContext _context;

    public PayrollCalculationService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates (or regenerates) a payslip for the given employee/month/year.
    /// Throws if a non-Draft payslip already exists.
    /// </summary>
    public async Task<Payslip> GeneratePayslipAsync(int employeeId, int month, int year)
    {
        // Check for existing
        var existing = await _context.Payslips
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId
                                   && p.Month == month && p.Year == year);

        if (existing != null)
        {
            if (existing.Status != PayslipStatus.Draft)
                throw new InvalidOperationException(
                    $"Payslip for {month}/{year} is already {existing.Status} and cannot be regenerated.");

            // Remove old draft so we can regenerate fresh
            _context.Payslips.Remove(existing);
            await _context.SaveChangesAsync();
        }

        // ── 1. Load employee + department ─────────────────────────────────────
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive)
            ?? throw new KeyNotFoundException("Active employee not found.");

        decimal baseSalary = employee.Department.BaseSalary;
        var lineItems = new List<PayslipLineItem>();

        // ── 2. Allowances (Global + Department + Individual) ──────────────────
        var allowances = await _context.Allowances
            .Where(a => a.Scope == AllowanceScope.Global
                     || (a.Scope == AllowanceScope.Department && a.DepartmentId == employee.DepartmentId)
                     || (a.Scope == AllowanceScope.Individual && a.EmployeeId == employeeId))
            .ToListAsync();

        foreach (var a in allowances)
            lineItems.Add(new PayslipLineItem
            {
                Category = "Allowance",
                Label = a.Name,
                Amount = a.Amount
            });

        decimal totalAllowances = allowances.Sum(a => a.Amount);

        // ── 3. Bonuses (Global + Department + Individual) ─────────────────────
        var bonuses = await _context.Bonuses
            .Where(b => b.Scope == BonusScope.Global
                     || (b.Scope == BonusScope.Department && b.DepartmentId == employee.DepartmentId)
                     || (b.Scope == BonusScope.Individual && b.EmployeeId == employeeId))
            .ToListAsync();

        decimal totalBonuses = 0m;
        foreach (var b in bonuses)
        {
            decimal amount = b.Type == BonusType.Percentage
                ? Math.Round(baseSalary * b.Value / 100, 2)
                : b.Value;

            string label = b.Type == BonusType.Percentage
                ? $"{b.Name} ({b.Value}%)"
                : b.Name;

            totalBonuses += amount;
            lineItems.Add(new PayslipLineItem
            {
                Category = "Bonus",
                Label = label,
                Amount = amount
            });
        }

        // ── 4. Deductions (Global + Department + Individual) ──────────────────
        var deductions = await _context.Deductions
            .Where(d => d.Scope == DeductionScope.Global
                     || (d.Scope == DeductionScope.Department && d.DepartmentId == employee.DepartmentId)
                     || (d.Scope == DeductionScope.Individual && d.EmployeeId == employeeId))
            .ToListAsync();

        foreach (var d in deductions)
            lineItems.Add(new PayslipLineItem
            {
                Category = "Deduction",
                Label = d.Name,
                Amount = -d.Amount          // Negative → deduction
            });

        decimal totalDeductions = deductions.Sum(d => d.Amount);

        // ── 5. Attendance penalty via rule engine ──────────────────────────────
        var attendance = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId
                     && a.Date.Month == month && a.Date.Year == year)
            .ToListAsync();

        int absentCount = attendance.Count(a => a.Status == AttendanceStatus.Absent);
        int lateCount   = attendance.Count(a => a.Status == AttendanceStatus.Late);
        int workingDays = attendance.Count;

        // Daily salary = baseSalary / working days in month
        // Use calendar working days (Mon–Fri) as fallback if no records exist
        int calendarWorkingDays = workingDays > 0
            ? workingDays
            : CountWeekdays(month, year);

        decimal dailySalary = calendarWorkingDays > 0
            ? Math.Round(baseSalary / calendarWorkingDays, 4)
            : 0m;

        var rules = await _context.SalaryRules
            .Where(r => r.IsActive)
            .ToListAsync();

        decimal attendancePenalty = 0m;

        foreach (var rule in rules)
        {
            int occurrences = rule.ConditionType == RuleConditionType.Absent ? absentCount : lateCount;
            int triggers    = occurrences / rule.Threshold;           // integer division — full triggers only
            decimal penalty = Math.Round(triggers * rule.PenaltyDays * dailySalary, 2);

            if (penalty > 0)
            {
                attendancePenalty += penalty;
                string label = rule.ConditionType == RuleConditionType.Absent
                    ? $"Absent Penalty ({occurrences} days × rule)"
                    : $"Late Penalty ({occurrences} lates ÷ {rule.Threshold} × rule)";

                lineItems.Add(new PayslipLineItem
                {
                    Category = "AttendancePenalty",
                    Label = label,
                    Amount = -penalty
                });
            }
        }

        // ── 6. Net Payable ────────────────────────────────────────────────────
        decimal netPayable = baseSalary
                           + totalAllowances
                           + totalBonuses
                           - totalDeductions
                           - attendancePenalty;

        var payslip = new Payslip
        {
            EmployeeId     = employeeId,
            Month          = month,
            Year           = year,
            BaseSalary     = Math.Round(baseSalary, 2),
            TotalAllowances = Math.Round(totalAllowances, 2),
            TotalBonuses    = Math.Round(totalBonuses, 2),
            TotalDeductions = Math.Round(totalDeductions, 2),
            AttendancePenalty = Math.Round(attendancePenalty, 2),
            NetPayable     = Math.Round(netPayable, 2),
            Status         = PayslipStatus.Draft,
            GeneratedAt    = DateTime.UtcNow,
            LineItems      = lineItems
        };

        _context.Payslips.Add(payslip);
        await _context.SaveChangesAsync();

        return payslip;
    }

    private static int CountWeekdays(int month, int year)
    {
        int days = DateTime.DaysInMonth(year, month);
        int count = 0;
        for (int d = 1; d <= days; d++)
        {
            var dow = new DateTime(year, month, d).DayOfWeek;
            if (dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday)
                count++;
        }
        return count;
    }
}
