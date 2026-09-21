namespace LiftAndShift.Core.Lifts;

public class Lift : SmartEnum<Lift>
{
  public static readonly Lift Squat = new(nameof(Squat), 1, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3, progressionIncrementKg: 5m);
  public static readonly Lift Press = new(nameof(Press), 2, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3, progressionIncrementKg: 2.5m);
  public static readonly Lift BenchPress = new(nameof(BenchPress), 3, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3, progressionIncrementKg: 2.5m);
  public static readonly Lift Deadlift = new(nameof(Deadlift), 4, openingWeightKg: 70m, incrementKg: 10m, workSetCount: 1, progressionIncrementKg: 5m);
  public static readonly Lift Row = new(nameof(Row), 5, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3, progressionIncrementKg: 5m);
  public static readonly Lift LatPulldown = new(nameof(LatPulldown), 6, openingWeightKg: 20m, incrementKg: 5m, workSetCount: 3, progressionIncrementKg: 5m);

  public decimal OpeningWeightKg { get; }
  public decimal IncrementKg { get; }
  public int WorkSetCount { get; }

  /// <summary>
  /// The amount Work Weight moves after a successful session for this lift. Distinct from
  /// <see cref="IncrementKg"/>, which is only used to climb the Ramp ladder to the opening Work Weight.
  /// </summary>
  public decimal ProgressionIncrementKg { get; }

  protected Lift(string name, int value, decimal openingWeightKg, decimal incrementKg, int workSetCount, decimal progressionIncrementKg) : base(name, value)
  {
    OpeningWeightKg = openingWeightKg;
    IncrementKg = incrementKg;
    WorkSetCount = workSetCount;
    ProgressionIncrementKg = progressionIncrementKg;
  }

  public decimal RampWeightForSet(int setNumber)
  {
    Guard.Against.NegativeOrZero(setNumber, nameof(setNumber));
    return OpeningWeightKg + (setNumber - 1) * IncrementKg;
  }
}
