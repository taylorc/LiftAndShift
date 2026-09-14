namespace LiftAndShift.UnitTests.Core.FamilyMemberAggregate;

public class FamilyMemberConstructor
{
  private readonly FamilyMemberName _testName = FamilyMemberName.From("Ada");
  private readonly Pin _testPin = Pin.From("1234");

  private FamilyMember CreateFamilyMember()
  {
    return new FamilyMember(_testName, _testPin);
  }

  [Fact]
  public void InitializesName()
  {
    var familyMember = CreateFamilyMember();

    familyMember.Name.ShouldBe(_testName);
  }

  [Fact]
  public void InitializesPin()
  {
    var familyMember = CreateFamilyMember();

    familyMember.Pin.ShouldBe(_testPin);
  }
}
