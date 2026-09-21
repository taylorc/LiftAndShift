namespace LiftAndShift.Web.WorkoutSessions;

public record WorkoutSessionRecord(
  int Id,
  int FamilyMemberId,
  string Workout,
  int TrainingPhase,
  DateOnly PerformedOn,
  IReadOnlyList<LoggedSetRecord> LoggedSets,
  IReadOnlyList<LiftOutcomeRecord> LiftOutcomes);
