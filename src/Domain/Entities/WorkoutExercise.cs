namespace LiftAndShift.Domain.Entities;

public class WorkoutExercise : BaseAuditableEntity
{
    public int WorkoutSessionId { get; set; }

    public WorkoutSession WorkoutSession { get; set; } = null!;

    public int ExerciseId { get; set; }

    public Exercise Exercise { get; set; } = null!;

    public int OrderIndex { get; set; }

    public string? Notes { get; set; }

    public IList<WorkoutSet> Sets { get; private set; } = new List<WorkoutSet>();
}
