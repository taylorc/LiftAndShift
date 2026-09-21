namespace LiftAndShift.UnitTests.Core.WorkWeightAggregate;

public class WeightKgFromTests
{
  [Fact]
  public void CreatesGivenPositiveValue()
  {
    var weightKg = WeightKg.From(30);
    weightKg.Value.ShouldBe(30);
  }

  [Fact]
  public void ThrowsGivenZero()
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => WeightKg.From(0));
  }

  [Fact]
  public void ThrowsGivenNegativeValue()
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => WeightKg.From(-1));
  }
}
