namespace PayrollAPI.Models;

public class Employee
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DepartmentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Allowance> Allowances { get; set; } = new List<Allowance>();
    public ICollection<Bonus> Bonuses { get; set; } = new List<Bonus>();
    public ICollection<Deduction> Deductions { get; set; } = new List<Deduction>();
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
