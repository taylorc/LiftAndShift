namespace LiftAndShift.UnitTests.Core.WorkoutSessionAggregate;

public class WorkoutSessionLogTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly DateOnly _testPerformedOn = new(2026, 9, 21);

  private static LoggedLiftInput Input(Lift lift, decimal weightKg, params int[] repsPerSet) =>
    new(lift, WeightKg.From(weightKg), repsPerSet);

  [Fact]
  public void LogsWorkoutAWithAllThreeLifts()
  {
    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 5),
        Input(Lift.Press, 25, 5, 5, 5),
        Input(Lift.Deadlift, 90, 5)
      ]);

    session.FamilyMemberId.ShouldBe(_testFamilyMemberId);
    session.Workout.ShouldBe(Workout.A);
    session.TrainingPhase.ShouldBe(TrainingPhase.From(1));
    session.PerformedOn.ShouldBe(_testPerformedOn);
    session.LoggedSets.Count.ShouldBe(7);
  }

  [Fact]
  public void LogsWorkoutBWithPhaseDependentThirdLift()
  {
    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.B, TrainingPhase.From(2), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 5),
        Input(Lift.BenchPress, 25, 5, 5, 5),
        Input(Lift.Row, 40, 5, 5, 5)
      ]);

    session.LoggedSets.Select(s => s.Lift).Distinct().ShouldBe([Lift.Squat, Lift.BenchPress, Lift.Row], ignoreOrder: true);
  }

  [Fact]
  public void RecordsSetNumberAndRepsAchievedPerSet()
  {
    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 3),
        Input(Lift.Press, 25, 5, 5, 5),
        Input(Lift.Deadlift, 90, 5)
      ]);

    var squatSets = session.LoggedSets.Where(s => s.Lift == Lift.Squat).OrderBy(s => s.SetNumber).ToList();
    squatSets.Select(s => s.SetNumber).ShouldBe([1, 2, 3]);
    squatSets.Select(s => s.RepsAchieved).ShouldBe([5, 5, 3]);
  }

  [Fact]
  public void ThrowsGivenALiftThatDoesNotBelongToTheWorkout()
  {
    Should.Throw<ArgumentException>(() => WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 5),
        Input(Lift.Press, 25, 5, 5, 5),
        Input(Lift.BenchPress, 25, 5, 5, 5)
      ]));
  }

  [Fact]
  public void ThrowsGivenAMissingLift()
  {
    Should.Throw<ArgumentException>(() => WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 5),
        Input(Lift.Press, 25, 5, 5, 5)
      ]));
  }

  [Fact]
  public void ThrowsGivenADuplicateLift()
  {
    Should.Throw<ArgumentException>(() => WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 5),
        Input(Lift.Squat, 30, 5, 5, 5),
        Input(Lift.Deadlift, 90, 5)
      ]));
  }

  [Fact]
  public void ThrowsGivenWrongNumberOfSetsForALift()
  {
    Should.Throw<ArgumentException>(() => WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5), // only 2 sets, needs 3
        Input(Lift.Press, 25, 5, 5, 5),
        Input(Lift.Deadlift, 90, 5)
      ]));
  }
}
