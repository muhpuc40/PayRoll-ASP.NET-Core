namespace PayrollAPI.Models.DTOs;

public class AttendanceDto
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
}