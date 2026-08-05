namespace LiftAndShift.Domain.Entities;

public class PersonalRecord : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;

    public int ExerciseId { get; set; }

    public Exercise Exercise { get; set; } = null!;

    public decimal WeightKg { get; set; }

    public int Reps { get; set; }

    public DateTimeOffset AchievedAt { get; set; }

    public decimal Estimated1RmKg { get; set; }
}
