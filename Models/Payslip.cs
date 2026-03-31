namespace PayrollAPI.Models;

public class Payslip
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    // Snapshot totals
    public decimal BaseSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalBonuses { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal AttendancePenalty { get; set; }
    public decimal NetPayable { get; set; }

    public PayslipStatus Status { get; set; } = PayslipStatus.Draft;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee Employee { get; set; } = null!;
    public ICollection<PayslipLineItem> LineItems { get; set; } = new List<PayslipLineItem>();
}

/// <summary>
/// Individual line on the payslip — e.g., "Housing Allowance: +1111"
/// </summary>
public class PayslipLineItem
{
    public int Id { get; set; }
    public int PayslipId { get; set; }
    public string Category { get; set; } = string.Empty;   // Allowance | Bonus | Deduction | AttendancePenalty
    public string Label { get; set; } = string.Empty;      // e.g. "Housing Allowance", "Bonus (15%)", "Tax"
    public decimal Amount { get; set; }                    // Positive = earning, Negative = deduction

    // Navigation
    public Payslip Payslip { get; set; } = null!;
}

public enum PayslipStatus
{
    Draft = 0,
    Approved = 1,
    Paid = 2
}
