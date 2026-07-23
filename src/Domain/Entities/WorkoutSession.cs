using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Domain.Entities;

public class WorkoutSession : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;

    public DateTimeOffset Date { get; set; }

    public string? Notes { get; set; }

    public WorkoutStatus Status { get; set; } = WorkoutStatus.Draft;

    public bool IsProgrammeSession { get; set; }

    public int? ProgrammeSessionId { get; set; }

    public IList<WorkoutExercise> Exercises { get; private set; } = new List<WorkoutExercise>();
}
