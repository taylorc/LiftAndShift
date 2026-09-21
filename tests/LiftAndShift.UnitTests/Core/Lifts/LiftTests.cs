namespace LiftAndShift.UnitTests.Core.Lifts;

public class LiftTests
{
  [Theory]
  [InlineData("Squat", 20, 5)]
  [InlineData("Press", 20, 5)]
  [InlineData("BenchPress", 20, 5)]
  [InlineData("Row", 20, 5)]
  [InlineData("LatPulldown", 20, 5)]
  [InlineData("Deadlift", 70, 10)]
  public void HasExpectedOpeningWeightAndIncrement(string liftName, decimal expectedOpeningWeightKg, decimal expectedIncrementKg)
  {
    var lift = Lift.FromName(liftName);

    lift.OpeningWeightKg.ShouldBe(expectedOpeningWeightKg);
    lift.IncrementKg.ShouldBe(expectedIncrementKg);
  }

  [Fact]
  public void RampWeightForSet1ReturnsOpeningWeight()
  {
    Lift.Squat.RampWeightForSet(1).ShouldBe(20m);
    Lift.Deadlift.RampWeightForSet(1).ShouldBe(70m);
  }

  [Fact]
  public void RampWeightForSet3AddsTwoIncrements()
  {
    Lift.Squat.RampWeightForSet(3).ShouldBe(30m);
    Lift.Deadlift.RampWeightForSet(3).ShouldBe(90m);
  }

  [Fact]
  public void ThrowsGivenSetNumberZero()
  {
    Assert.Throws<ArgumentException>(() => Lift.Squat.RampWeightForSet(0));
  }
}
