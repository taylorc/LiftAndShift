using LiftAndShift.UseCases.WorkoutSessions;

namespace LiftAndShift.Web.WorkoutSessions;

public record WorkoutSessionRecord(
  int Id,
  int FamilyMemberId,
  string Workout,
  int TrainingPhase,
  DateOnly PerformedOn,
  IReadOnlyList<LoggedSetRecord> LoggedSets,
  IReadOnlyList<LiftOutcomeRecord> LiftOutcomes)
{
  public static WorkoutSessionRecord FromDto(WorkoutSessionDto e) =>
    new(
      e.Id.Value,
      e.FamilyMemberId.Value,
      e.Workout.Name,
      e.TrainingPhase.Value,
      e.PerformedOn,
      e.LoggedSets.Select(s => new LoggedSetRecord(s.Id.Value, s.Lift.Name, s.WeightKg.Value, s.SetNumber, s.RepsAchieved)).ToList(),
      e.LiftOutcomes.Select(o => new LiftOutcomeRecord(o.Lift.Name, o.Successful, o.NewWeightKg.Value, o.Deloaded)).ToList());
}
