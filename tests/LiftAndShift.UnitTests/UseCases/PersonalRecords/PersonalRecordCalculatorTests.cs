namespace LiftAndShift.UnitTests.UseCases.PersonalRecords;

public class PersonalRecordCalculatorTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);

  private WorkoutSession SuccessfulSquatSession(DateOnly performedOn, decimal weightKg) =>
    WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), performedOn,
      [
        new LoggedLiftInput(Lift.Squat, WeightKg.From(weightKg), [5, 5, 5]),
        new LoggedLiftInput(Lift.Press, WeightKg.From(20), [5, 5, 5]),
        new LoggedLiftInput(Lift.Deadlift, WeightKg.From(70), [5])
      ]);

  private WorkoutSession FailedSquatSession(DateOnly performedOn, decimal weightKg) =>
    WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), performedOn,
      [
        new LoggedLiftInput(Lift.Squat, WeightKg.From(weightKg), [5, 5, 3]), // missed the last set
        new LoggedLiftInput(Lift.Press, WeightKg.From(20), [5, 5, 5]),
        new LoggedLiftInput(Lift.Deadlift, WeightKg.From(70), [5])
      ]);

  [Fact]
  public void PicksTheHeaviestSuccessfulSessionForALift()
  {
    var sessions = new[]
    {
      SuccessfulSquatSession(new DateOnly(2026, 1, 1), 60),
      SuccessfulSquatSession(new DateOnly(2026, 2, 1), 80),
      SuccessfulSquatSession(new DateOnly(2026, 1, 15), 70)
    };

    var records = PersonalRecordCalculator.Calculate(sessions);

    var squatRecord = records.Single(r => r.Lift == Lift.Squat);
    squatRecord.WeightKg.ShouldBe(WeightKg.From(80));
    squatRecord.AchievedOn.ShouldBe(new DateOnly(2026, 2, 1));
  }

  [Fact]
  public void IgnoresAHeavierButFailedSession()
  {
    var sessions = new[]
    {
      SuccessfulSquatSession(new DateOnly(2026, 1, 1), 60),
      FailedSquatSession(new DateOnly(2026, 2, 1), 100) // heavier, but failed - doesn't count
    };

    var records = PersonalRecordCalculator.Calculate(sessions);

    var squatRecord = records.Single(r => r.Lift == Lift.Squat);
    squatRecord.WeightKg.ShouldBe(WeightKg.From(60));
  }

  [Fact]
  public void OmitsALiftWithOnlyFailedSessions()
  {
    var sessions = new[] { FailedSquatSession(new DateOnly(2026, 1, 1), 60) };

    var records = PersonalRecordCalculator.Calculate(sessions);

    records.ShouldNotContain(r => r.Lift == Lift.Squat);
  }

  [Fact]
  public void OnATieBreaksToTheEarlierDate()
  {
    var sessions = new[]
    {
      SuccessfulSquatSession(new DateOnly(2026, 2, 1), 80),
      SuccessfulSquatSession(new DateOnly(2026, 1, 1), 80) // same weight, earlier date
    };

    var records = PersonalRecordCalculator.Calculate(sessions);

    var squatRecord = records.Single(r => r.Lift == Lift.Squat);
    squatRecord.AchievedOn.ShouldBe(new DateOnly(2026, 1, 1));
  }

  [Fact]
  public void TracksEachLiftIndependently()
  {
    var sessions = new[] { SuccessfulSquatSession(new DateOnly(2026, 1, 1), 60) };

    var records = PersonalRecordCalculator.Calculate(sessions);

    records.Count.ShouldBe(3); // Squat, Press, Deadlift all succeeded in this one session
    records.ShouldContain(r => r.Lift == Lift.Squat && r.WeightKg == WeightKg.From(60));
    records.ShouldContain(r => r.Lift == Lift.Press && r.WeightKg == WeightKg.From(20));
    records.ShouldContain(r => r.Lift == Lift.Deadlift && r.WeightKg == WeightKg.From(70));
  }

  [Fact]
  public void ReturnsEmptyGivenNoSessions()
  {
    var records = PersonalRecordCalculator.Calculate([]);

    records.ShouldBeEmpty();
  }
}
