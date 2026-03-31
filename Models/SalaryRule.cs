namespace PayrollAPI.Models;

/// <summary>
/// Configurable salary penalty rules stored in the database.
///
/// Examples:
///   ConditionType=Absent, Threshold=1, PenaltyDays=1
///     → Each absent day deducts 1 day's salary
///
///   ConditionType=Late, Threshold=3, PenaltyDays=1
///     → Every 3 late marks deduct 1 day's salary
/// </summary>
public class SalaryRule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;          // Human-readable label
    public RuleConditionType ConditionType { get; set; }      // Absent | Late
    public int Threshold { get; set; } = 1;                   // How many occurrences trigger the penalty
    public decimal PenaltyDays { get; set; } = 1;             // Days of salary to deduct per trigger
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum RuleConditionType
{
    Absent = 0,
    Late = 1
}
