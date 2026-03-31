namespace PayrollAPI.Models;

public class Bonus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BonusType Type { get; set; } = BonusType.Fixed; // Fixed | Percentage
    public decimal Value { get; set; }  // Flat amount OR percentage value (e.g. 15 = 15%)
    public BonusScope Scope { get; set; } = BonusScope.Global;

    // Nullable FK — only one set based on Scope
    public int? DepartmentId { get; set; }
    public int? EmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Department? Department { get; set; }
    public Employee? Employee { get; set; }
}

public enum BonusType
{
    Fixed = 0,
    Percentage = 1
}

public enum BonusScope
{
    Global = 0,
    Department = 1,
    Individual = 2
}
