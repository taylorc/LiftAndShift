namespace LiftAndShift.Core.Workouts;

public class Workout : SmartEnum<Workout>
{
  public static readonly Workout A = new(nameof(A), 1);
  public static readonly Workout B = new(nameof(B), 2);

  protected Workout(string name, int value) : base(name, value) { }
}
