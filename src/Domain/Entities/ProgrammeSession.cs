using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Domain.Entities;

public class ProgrammeSession : BaseAuditableEntity
{
    public int UserProgrammeId { get; set; }

    public UserProgramme UserProgramme { get; set; } = null!;

    public int? WorkoutSessionId { get; set; }

    public WorkoutType WorkoutType { get; set; }

    public DateTimeOffset ScheduledDate { get; set; }

    public DateTimeOffset? CompletedDate { get; set; }

    /// <summary>
    /// JSON column: current weights per lift (e.g. { "Squat": 100.0, "Bench Press": 60.0 })
    /// </summary>
    public Dictionary<string, decimal> LiftProgression { get; set; } = new();

    /// <summary>
    /// JSON column: consecutive session failures per lift, carried forward from the previous session
    /// and used to trigger a deload once a lift reaches 3 (see StartingStrengthProgressionService).
    /// </summary>
    public Dictionary<string, int> ConsecutiveFailures { get; set; } = new();
}
