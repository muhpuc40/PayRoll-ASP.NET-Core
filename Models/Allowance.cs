namespace PayrollAPI.Models;

/// <summary>
/// Scope: Global = applies to all employees, Department = scoped to a department,
/// Individual = scoped to one employee.
/// Exactly one of DepartmentId / EmployeeId is set (or neither for Global).
/// </summary>
public class Allowance
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // e.g. Housing, Transport
    public decimal Amount { get; set; }
    public AllowanceScope Scope { get; set; } = AllowanceScope.Global;

    // Nullable FK — only one is set based on Scope
    public int? DepartmentId { get; set; }
    public int? EmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Department? Department { get; set; }
    public Employee? Employee { get; set; }
}

public enum AllowanceScope
{
    Global = 0,
    Department = 1,
    Individual = 2
}
