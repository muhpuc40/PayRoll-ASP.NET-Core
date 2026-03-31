namespace PayrollAPI.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Allowance> Allowances { get; set; } = new List<Allowance>();
    public ICollection<Bonus> Bonuses { get; set; } = new List<Bonus>();
    public ICollection<Deduction> Deductions { get; set; } = new List<Deduction>();
}
