namespace LiftAndShift.UnitTests.Core.ProgrammeAggregate;

public class TrainingPhaseFromTests
{
  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  public void CreatesGivenValidValue(int validValue)
  {
    var trainingPhase = TrainingPhase.From(validValue);
    Assert.Equal(validValue, trainingPhase.Value);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(4)]
  [InlineData(-1)]
  public void ThrowsGivenInvalidValue(int invalidValue)
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => TrainingPhase.From(invalidValue));
  }
}
