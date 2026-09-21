using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.ProgrammeAggregate;

namespace LiftAndShift.Core.Workouts;

public static class WorkoutLifts
{
  public static IReadOnlyList<Lift> For(Workout workout, TrainingPhase trainingPhase)
  {
    if (workout == Workout.A)
    {
      return [Lift.Squat, Lift.Press, Lift.Deadlift];
    }

    if (workout == Workout.B)
    {
      return [Lift.Squat, Lift.BenchPress, PhaseLift(trainingPhase)];
    }

    throw new ArgumentOutOfRangeException(nameof(workout), workout, "Unknown workout.");
  }

  private static Lift PhaseLift(TrainingPhase trainingPhase) => trainingPhase.Value switch
  {
    1 => Lift.Deadlift,
    2 => Lift.Row,
    3 => Lift.LatPulldown,
    _ => throw new ArgumentOutOfRangeException(nameof(trainingPhase), trainingPhase, "Unknown Training Phase.")
  };
}
