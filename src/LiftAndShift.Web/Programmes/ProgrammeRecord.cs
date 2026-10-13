using LiftAndShift.UseCases.Programmes;

namespace LiftAndShift.Web.Programmes;

// WorkoutLifts: the lifts each Workout means at this Training Phase, keyed by Workout name ("A", "B"),
// resolved by Core's WorkoutLifts.For so clients never re-derive the phase-to-third-lift mapping.
public record ProgrammeRecord(int Id, int FamilyMemberId, int TrainingPhase, IReadOnlyDictionary<string, IReadOnlyList<string>> WorkoutLifts)
{
  public static ProgrammeRecord FromDto(ProgrammeDto e) =>
    new(
      e.Id.Value,
      e.FamilyMemberId.Value,
      e.TrainingPhase.Value,
      Core.Workouts.Workout.List.ToDictionary(
        workout => workout.Name,
        workout => (IReadOnlyList<string>)Core.Workouts.WorkoutLifts.For(workout, e.TrainingPhase).Select(lift => lift.Name).ToList()));
}
