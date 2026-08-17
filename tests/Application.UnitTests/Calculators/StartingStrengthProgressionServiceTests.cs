using LiftAndShift.Application.Calculators;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Calculators;

[TestFixture]
public class StartingStrengthProgressionServiceTests
{
    [Test]
    public void ShouldIncrementSquatBy5Kg()
    {
        var next = StartingStrengthProgressionService.NextWeight("Squat", 100m, 0);

        next.ShouldBe(105m);
    }

    [Test]
    public void ShouldIncrementBenchBy2_5Kg()
    {
        var next = StartingStrengthProgressionService.NextWeight("Bench Press", 60m, 0);

        next.ShouldBe(62.5m);
    }

    [Test]
    public void ShouldDeload_AfterThreeConsecutiveFailures()
    {
        var next = StartingStrengthProgressionService.NextWeight("Squat", 100m, 3);

        // 10% deload → 90 kg
        next.ShouldBe(90m);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void ShouldHoldWeight_AfterOneOrTwoFailures(int failures)
    {
        // With 1 or 2 failures the weight is held steady (not incremented, not yet a deload)
        var next = StartingStrengthProgressionService.NextWeight("Squat", 100m, failures);

        next.ShouldBe(100m);
    }

    [Test]
    public void ShouldRoundToNearest1_25Kg()
    {
        // 90% of 83 = 74.7 → nearest 1.25 = 75.0
        var next = StartingStrengthProgressionService.NextWeight("Squat", 83m, 3);

        (next % 1.25m).ShouldBe(0m, $"Result {next} should be a multiple of 1.25");
    }
}
