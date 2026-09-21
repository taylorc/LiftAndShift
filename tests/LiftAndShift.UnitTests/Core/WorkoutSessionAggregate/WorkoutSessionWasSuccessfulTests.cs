namespace LiftAndShift.UnitTests.Core.WorkoutSessionAggregate;

public class WorkoutSessionWasSuccessfulTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly DateOnly _testPerformedOn = new(2026, 9, 21);

  private static LoggedLiftInput Input(Lift lift, decimal weightKg, params int[] repsPerSet) =>
    new(lift, WeightKg.From(weightKg), repsPerSet);

  [Fact]
  public void IsSuccessfulWhenEverySetHitsExactlyTheTarget()
  {
    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 5),
        Input(Lift.Press, 25, 5, 5, 5),
        Input(Lift.Deadlift, 90, 5)
      ]);

    session.WasSuccessful(Lift.Squat).ShouldBeTrue();
    session.WasSuccessful(Lift.Deadlift).ShouldBeTrue();
  }

  [Fact]
  public void IsSuccessfulWhenSetsExceedTheTarget()
  {
    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 6, 7, 5),
        Input(Lift.Press, 25, 5, 5, 5),
        Input(Lift.Deadlift, 90, 5)
      ]);

    session.WasSuccessful(Lift.Squat).ShouldBeTrue();
  }

  [Fact]
  public void IsNotSuccessfulWhenAnySetMissesTheTarget()
  {
    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 3),
        Input(Lift.Press, 25, 5, 5, 5),
        Input(Lift.Deadlift, 90, 5)
      ]);

    session.WasSuccessful(Lift.Squat).ShouldBeFalse();
    session.WasSuccessful(Lift.Press).ShouldBeTrue();
  }

  [Fact]
  public void IsNotSuccessfulWhenDeadliftsSingleSetMissesTheTarget()
  {
    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), _testPerformedOn,
      [
        Input(Lift.Squat, 30, 5, 5, 5),
        Input(Lift.Press, 25, 5, 5, 5),
        Input(Lift.Deadlift, 90, 4)
      ]);

    session.WasSuccessful(Lift.Deadlift).ShouldBeFalse();
  }
}
