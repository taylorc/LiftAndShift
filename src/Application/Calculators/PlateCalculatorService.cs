namespace LiftAndShift.Application.Calculators;

public class PlateResult
{
    public bool IsExact { get; set; }
    public decimal ActualWeightKg { get; set; }
    public Dictionary<decimal, int> PlatesPerSide { get; set; } = new();
}

public class PlateCalculatorService
{
    public PlateResult Calculate(decimal targetKg, decimal barKg, IEnumerable<decimal> availablePlateSizes)
    {
        var plates = availablePlateSizes.OrderByDescending(p => p).ToList();
        decimal weightPerSide = (targetKg - barKg) / 2m;

        if (weightPerSide < 0)
        {
            return new PlateResult
            {
                IsExact = targetKg == barKg,
                ActualWeightKg = barKg,
                PlatesPerSide = new Dictionary<decimal, int>()
            };
        }

        var platesPerSide = new Dictionary<decimal, int>();
        decimal remaining = weightPerSide;

        foreach (var plate in plates)
        {
            int count = (int)(remaining / plate);
            if (count > 0)
            {
                platesPerSide[plate] = count;
                remaining -= plate * count;
            }
        }

        decimal actualPerSide = weightPerSide - remaining;
        decimal actualTotal = barKg + (actualPerSide * 2m);
        bool isExact = remaining == 0m;

        return new PlateResult
        {
            IsExact = isExact,
            ActualWeightKg = actualTotal,
            PlatesPerSide = platesPerSide
        };
    }
}
