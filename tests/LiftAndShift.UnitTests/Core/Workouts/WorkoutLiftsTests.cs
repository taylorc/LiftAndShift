namespace LiftAndShift.UnitTests.Core.Workouts;

public class WorkoutLiftsTests
{
  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  public void WorkoutAIsAlwaysSquatPressDeadliftRegardlessOfPhase(int trainingPhase)
  {
    var lifts = WorkoutLifts.For(Workout.A, TrainingPhase.From(trainingPhase));

    lifts.ShouldBe([Lift.Squat, Lift.Press, Lift.Deadlift]);
  }

  [Fact]
  public void WorkoutBAtPhase1EndsWithDeadlift()
  {
    var lifts = WorkoutLifts.For(Workout.B, TrainingPhase.From(1));

    lifts.ShouldBe([Lift.Squat, Lift.BenchPress, Lift.Deadlift]);
  }

  [Fact]
  public void WorkoutBAtPhase2EndsWithRow()
  {
    var lifts = WorkoutLifts.For(Workout.B, TrainingPhase.From(2));

    lifts.ShouldBe([Lift.Squat, Lift.BenchPress, Lift.Row]);
  }

  [Fact]
  public void WorkoutBAtPhase3EndsWithLatPulldown()
  {
    var lifts = WorkoutLifts.For(Workout.B, TrainingPhase.From(3));

    lifts.ShouldBe([Lift.Squat, Lift.BenchPress, Lift.LatPulldown]);
  }
}
