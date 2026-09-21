namespace LiftAndShift.Core.Lifts;

public class Lift : SmartEnum<Lift>
{
  public static readonly Lift Squat = new(nameof(Squat), 1, openingWeightKg: 20m, incrementKg: 5m);
  public static readonly Lift Press = new(nameof(Press), 2, openingWeightKg: 20m, incrementKg: 5m);
  public static readonly Lift BenchPress = new(nameof(BenchPress), 3, openingWeightKg: 20m, incrementKg: 5m);
  public static readonly Lift Deadlift = new(nameof(Deadlift), 4, openingWeightKg: 70m, incrementKg: 10m);
  public static readonly Lift Row = new(nameof(Row), 5, openingWeightKg: 20m, incrementKg: 5m);
  public static readonly Lift LatPulldown = new(nameof(LatPulldown), 6, openingWeightKg: 20m, incrementKg: 5m);

  public decimal OpeningWeightKg { get; }
  public decimal IncrementKg { get; }

  protected Lift(string name, int value, decimal openingWeightKg, decimal incrementKg) : base(name, value)
  {
    OpeningWeightKg = openingWeightKg;
    IncrementKg = incrementKg;
  }

  public decimal RampWeightForSet(int setNumber)
  {
    Guard.Against.NegativeOrZero(setNumber, nameof(setNumber));
    return OpeningWeightKg + (setNumber - 1) * IncrementKg;
  }
}
