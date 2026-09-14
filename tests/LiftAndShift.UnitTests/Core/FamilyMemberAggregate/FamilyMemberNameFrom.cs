namespace LiftAndShift.UnitTests.Core.FamilyMemberAggregate;

public class FamilyMemberNameFrom
{
  [Fact]
  public void CreatesGivenValidValue()
  {
    string validValue = "Ada";
    var familyMemberName = FamilyMemberName.From(validValue);
    Assert.Equal(validValue, familyMemberName.Value);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  public void ThrowsGivenInvalidValue(string? invalidValue)
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => FamilyMemberName.From(invalidValue!));
  }
}
