using LiftAndShift.Application.Calculators;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Calculators;

[TestFixture]
public class WarmupCalculatorServiceTests
{
    private WarmupCalculatorService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new WarmupCalculatorService();
    }

    [Test]
    public void ShouldReturnEmptyBarSetFirst()
    {
        var result = _sut.Calculate(100m);

        result.ShouldNotBeEmpty();
        result[0].WeightKg.ShouldBe(20m);
    }

    [Test]
    public void ShouldCalculateCorrectPercentages()
    {
        // Working weight = 100 kg, bar = 20 kg
        // 40% = 40 kg, 60% = 60 kg, 80% = 80 kg
        var result = _sut.Calculate(100m, 20m, 4);

        result.Count.ShouldBe(4);
        result[0].WeightKg.ShouldBe(20m);    // empty bar
        result[1].WeightKg.ShouldBe(40m);    // 40%
        result[2].WeightKg.ShouldBe(60m);    // 60%
        result[3].WeightKg.ShouldBe(80m);    // 80%
    }

    [Test]
    public void ShouldRoundToNearest1_25Kg()
    {
        // 60% of 83 kg = 49.8 → nearest 1.25 = 50.0
        var result = _sut.Calculate(83m, 20m, 4);

        foreach (var set in result)
        {
            (set.WeightKg % 1.25m).ShouldBe(0m,
                $"Set {set.SetNumber} weight {set.WeightKg} is not a multiple of 1.25");
        }
    }

    [Test]
    public void ShouldCollapseStepsWhenWeightBelowBar()
    {
        // Very light working weight — 40% and 60% will be below bar
        var result = _sut.Calculate(30m, 20m, 4);

        // Should not have duplicate bar-weight entries
        var barSets = result.Where(s => s.WeightKg == 20m).ToList();
        barSets.Count.ShouldBe(1, "Duplicate bar-weight sets should be collapsed");
    }

    [Test]
    public void ShouldDefaultTo4Steps()
    {
        var result = _sut.Calculate(120m);

        result.Count.ShouldBe(4);
    }
}
