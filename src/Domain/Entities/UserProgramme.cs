using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Domain.Entities;

public class UserProgramme : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;

    public string ProgrammeTemplateId { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    public ProgrammeStatus Status { get; set; } = ProgrammeStatus.Active;

    public int SessionCount { get; set; }

    public WorkoutType CurrentWorkoutType { get; set; } = WorkoutType.A;

    public IList<ProgrammeSession> Sessions { get; private set; } = new List<ProgrammeSession>();
}
