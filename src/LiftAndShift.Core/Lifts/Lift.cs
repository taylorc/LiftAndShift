namespace LiftAndShift.Core.Lifts;

public class Lift : SmartEnum<Lift>
{
  public static readonly Lift Squat = new(nameof(Squat), 1, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3);
  public static readonly Lift Press = new(nameof(Press), 2, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3);
  public static readonly Lift BenchPress = new(nameof(BenchPress), 3, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3);
  public static readonly Lift Deadlift = new(nameof(Deadlift), 4, openingWeightKg: 70m, incrementKg: 10m, workSetCount: 1);
  public static readonly Lift Row = new(nameof(Row), 5, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3);
  public static readonly Lift LatPulldown = new(nameof(LatPulldown), 6, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3);

  public decimal OpeningWeightKg { get; }
  public decimal IncrementKg { get; }
  public int WorkSetCount { get; }

  protected Lift(string name, int value, decimal openingWeightKg, decimal incrementKg, int workSetCount) : base(name, value)
  {
    OpeningWeightKg = openingWeightKg;
    IncrementKg = incrementKg;
    WorkSetCount = workSetCount;
  }

  public decimal RampWeightForSet(int setNumber)
  {
    Guard.Against.NegativeOrZero(setNumber, nameof(setNumber));
    return OpeningWeightKg + (setNumber - 1) * IncrementKg;
  }
}
