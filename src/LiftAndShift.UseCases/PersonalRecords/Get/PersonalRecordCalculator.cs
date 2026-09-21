using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkoutSessionAggregate;

namespace LiftAndShift.UseCases.PersonalRecords.Get;

/// <summary>
/// Pure, database-free personal-record selection: for each lift, the heaviest weight logged in a
/// session where every set for that lift hit its rep target - reusing the exact same success
/// definition Progression/Deload already relies on (<see cref="WorkoutSession.WasSuccessful"/>).
/// </summary>
public static class PersonalRecordCalculator
{
  public static IReadOnlyList<PersonalRecordDto> Calculate(IEnumerable<WorkoutSession> sessions)
  {
    var recordsByLift = new Dictionary<Lift, PersonalRecordDto>();

    foreach (var session in sessions)
    {
      var liftsInSession = session.LoggedSets.Select(s => s.Lift).Distinct();

      foreach (var lift in liftsInSession)
      {
        if (!session.WasSuccessful(lift)) continue;

        var weightKg = session.LoggedSets.First(s => s.Lift == lift).WeightKg;
        var candidate = new PersonalRecordDto(lift, weightKg, session.PerformedOn, session.Id);

        if (!recordsByLift.TryGetValue(lift, out var current)
            || IsBetterRecord(candidate, current))
        {
          recordsByLift[lift] = candidate;
        }
      }
    }

    return recordsByLift.Values.ToList();
  }

  private static bool IsBetterRecord(PersonalRecordDto candidate, PersonalRecordDto current) =>
    candidate.WeightKg.Value > current.WeightKg.Value
    || (candidate.WeightKg.Value == current.WeightKg.Value && candidate.AchievedOn < current.AchievedOn);
}
