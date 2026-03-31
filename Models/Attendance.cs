namespace PayrollAPI.Models;

public class Attendance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee Employee { get; set; } = null!;
}

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2
}
