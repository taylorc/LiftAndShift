using LiftAndShift.Application.Calculators;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Endpoints;

public class Calculators : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetWarmupSets, "warmup");
        groupBuilder.MapGet(GetPlateCalculation, "plates");
    }

    [EndpointSummary("Calculate warmup sets for a working weight")]
    public static Ok<IReadOnlyList<WarmupSet>> GetWarmupSets(
        WarmupCalculatorService calculator,
        decimal weight,
        decimal bar = 20m,
        int steps = 4)
    {
        var result = calculator.Calculate(weight, bar, steps);
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Calculate plates needed for a target weight")]
    public static Ok<PlateResult> GetPlateCalculation(
        PlateCalculatorService calculator,
        decimal target,
        decimal bar = 20m,
        string? plates = null)
    {
        // Default plate sizes (standard gym set in kg)
        var availablePlates = plates != null
            ? plates.Split(',').Select(decimal.Parse)
            : new decimal[] { 25m, 20m, 15m, 10m, 5m, 2.5m, 1.25m };

        var result = calculator.Calculate(target, bar, availablePlates);
        return TypedResults.Ok(result);
    }
}
