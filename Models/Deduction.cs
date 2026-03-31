namespace PayrollAPI.Models;

public class Deduction
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // e.g. Tax, Penalty
    public decimal Amount { get; set; }
    public DeductionScope Scope { get; set; } = DeductionScope.Global;

    // Nullable FK — only one set based on Scope
    public int? DepartmentId { get; set; }
    public int? EmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Department? Department { get; set; }
    public Employee? Employee { get; set; }
}

public enum DeductionScope
{
    Global = 0,
    Department = 1,
    Individual = 2
}
