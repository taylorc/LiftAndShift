namespace LiftAndShift.Application.Calculators;

public class WarmupSet
{
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
}

public class WarmupCalculatorService
{
    private static decimal RoundToNearest1_25(decimal value)
    {
        return Math.Round(value / 1.25m, MidpointRounding.AwayFromZero) * 1.25m;
    }

    public IReadOnlyList<WarmupSet> Calculate(decimal workingWeightKg, decimal barWeightKg = 20m, int steps = 4)
    {
        // Percentages and reps for each step
        var stepDefs = new (decimal pct, int reps)[]
        {
            (0m, 5),      // empty bar
            (0.40m, 5),   // 40%
            (0.60m, 3),   // 60%
            (0.80m, 2),   // 80%
        };

        var result = new List<WarmupSet>();
        int setNumber = 1;

        // Take only the requested number of steps
        var effectiveSteps = stepDefs.Take(steps).ToArray();

        foreach (var (pct, reps) in effectiveSteps)
        {
            decimal weight = pct == 0m
                ? barWeightKg
                : RoundToNearest1_25(workingWeightKg * pct);

            // Collapse steps where calculated weight is at or below bar weight
            if (weight <= barWeightKg && pct > 0m)
            {
                weight = barWeightKg;
            }

            // Only add bar step once (collapse duplicates at bar weight)
            if (result.Count > 0 && result[^1].WeightKg == weight && weight == barWeightKg)
            {
                continue;
            }

            result.Add(new WarmupSet
            {
                SetNumber = setNumber++,
                Reps = reps,
                WeightKg = weight
            });
        }

        return result.AsReadOnly();
    }
}
