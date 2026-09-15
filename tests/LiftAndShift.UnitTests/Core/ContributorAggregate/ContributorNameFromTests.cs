namespace LiftAndShift.UnitTests.Core.ContributorAggregate;

public class ContributorNameFromTests
{
  [Fact]
  public void CreatesGivenValidValue()
  {
    string validValue = "ardalis";
    var contributorName = ContributorName.From(validValue);
    Assert.Equal(validValue, contributorName.Value);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  public void ThrowsGivenInvalidValue(string? invalidValue)
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => ContributorName.From(invalidValue!));
  }
}
