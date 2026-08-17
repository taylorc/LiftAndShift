namespace LiftAndShift.Application.Calculators;

public static class StartingStrengthProgressionService
{
    private static readonly HashSet<string> HeavyLifts = new(StringComparer.OrdinalIgnoreCase)
    {
        "Squat", "Deadlift"
    };

    private static decimal RoundToNearest1_25(decimal value)
    {
        return Math.Round(value / 1.25m, MidpointRounding.AwayFromZero) * 1.25m;
    }

    public static decimal NextWeight(string liftName, decimal currentWeight, int consecutiveFailures)
    {
        if (ShouldDeload(consecutiveFailures))
        {
            return RoundToNearest1_25(ApplyDeload(currentWeight));
        }

        if (consecutiveFailures > 0)
        {
            // 1st/2nd consecutive failure: hold weight steady rather than incrementing.
            return RoundToNearest1_25(currentWeight);
        }

        return RoundToNearest1_25(ApplyIncrement(liftName, currentWeight));
    }

    public static bool ShouldDeload(int consecutiveFailures) => consecutiveFailures >= 3;

    public static decimal ApplyIncrement(string liftName, decimal weight)
    {
        decimal increment = HeavyLifts.Contains(liftName) ? 5m : 2.5m;
        return weight + increment;
    }

    public static decimal ApplyDeload(decimal weight)
    {
        return weight * 0.9m;
    }
}
