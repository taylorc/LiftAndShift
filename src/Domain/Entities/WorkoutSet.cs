using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Domain.Entities;

public class WorkoutSet : BaseAuditableEntity
{
    public int WorkoutExerciseId { get; set; }

    public WorkoutExercise WorkoutExercise { get; set; } = null!;

    public int SetNumber { get; set; }

    public SetType SetType { get; set; } = SetType.WorkingSet;

    public decimal WeightKg { get; set; }

    public int Reps { get; set; }

    public int? CompletedReps { get; set; }

    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }
}
