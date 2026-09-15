namespace LiftAndShift.UnitTests.Core.FamilyMemberAggregate;

public class PinFromTests
{
  [Fact]
  public void CreatesGivenValidValue()
  {
    string validValue = "1234";
    var pin = Pin.From(validValue);
    Assert.Equal(validValue, pin.Value);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("123")]
  [InlineData("12345")]
  [InlineData("12a4")]
  public void ThrowsGivenInvalidValue(string? invalidValue)
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => Pin.From(invalidValue!));
  }
}
