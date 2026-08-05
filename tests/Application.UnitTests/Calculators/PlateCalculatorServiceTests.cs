using LiftAndShift.Application.Calculators;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Calculators;

[TestFixture]
public class PlateCalculatorServiceTests
{
    private PlateCalculatorService _sut = null!;
    private static readonly decimal[] StandardPlates = { 25m, 20m, 15m, 10m, 5m, 2.5m, 1.25m };

    [SetUp]
    public void SetUp()
    {
        _sut = new PlateCalculatorService();
    }

    [Test]
    public void ShouldCalculateCorrectPlatesForKnownWeight()
    {
        // 100 kg total, 20 kg bar → 40 kg per side → 25 + 15 = 40
        var result = _sut.Calculate(100m, 20m, StandardPlates);

        result.IsExact.ShouldBeTrue();
        result.ActualWeightKg.ShouldBe(100m);
        result.PlatesPerSide[25m].ShouldBe(1);
        result.PlatesPerSide[15m].ShouldBe(1);
    }

    [Test]
    public void ShouldReturnIsExactFalse_WhenExactWeightNotAchievable()
    {
        // Target 101 kg with standard plates (only multiples of 1.25 achievable)
        // 101 - 20 = 81 per side → not achievable without 0.5 kg plates
        var result = _sut.Calculate(101m, 20m, new decimal[] { 25m, 20m, 15m, 10m, 5m });

        result.IsExact.ShouldBeFalse();
    }

    [Test]
    public void ShouldHandleEmptyBarOnly()
    {
        var result = _sut.Calculate(20m, 20m, StandardPlates);

        result.IsExact.ShouldBeTrue();
        result.ActualWeightKg.ShouldBe(20m);
        result.PlatesPerSide.ShouldBeEmpty();
    }

    [Test]
    public void ShouldReturnZeroPlates_WhenTargetEqualsBarWeight()
    {
        var result = _sut.Calculate(20m, 20m, StandardPlates);

        // Implementation returns empty dictionary when no plates needed
        result.PlatesPerSide.ShouldBeEmpty("No plates should be used when target equals bar weight");
    }
}
