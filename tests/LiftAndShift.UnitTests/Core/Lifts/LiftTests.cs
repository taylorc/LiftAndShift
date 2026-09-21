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

  [Theory]
  [InlineData("Squat", 3)]
  [InlineData("Press", 3)]
  [InlineData("BenchPress", 3)]
  [InlineData("Row", 3)]
  [InlineData("LatPulldown", 3)]
  [InlineData("Deadlift", 1)]
  public void HasExpectedWorkSetCount(string liftName, int expectedWorkSetCount)
  {
    var lift = Lift.FromName(liftName);

    lift.WorkSetCount.ShouldBe(expectedWorkSetCount);
  }

  [Theory]
  [InlineData("Squat", 5)]
  [InlineData("Press", 2.5)]
  [InlineData("BenchPress", 2.5)]
  [InlineData("Row", 5)]
  [InlineData("LatPulldown", 5)]
  [InlineData("Deadlift", 5)]
  public void HasExpectedProgressionIncrement(string liftName, decimal expectedProgressionIncrementKg)
  {
    var lift = Lift.FromName(liftName);

    lift.ProgressionIncrementKg.ShouldBe(expectedProgressionIncrementKg);
  }
}
