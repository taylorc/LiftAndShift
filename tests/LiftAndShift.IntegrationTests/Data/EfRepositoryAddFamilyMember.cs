using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.IntegrationTests.Data;

public class EfRepositoryAddFamilyMember : BaseEfRepoTestFixture
{
  [Fact]
  public async Task AddsFamilyMemberAndSetsId()
  {
    var testName = FamilyMemberName.From("Ada");
    var testPin = Pin.From("1234");
    var repository = GetFamilyMemberRepository();
    var familyMember = new FamilyMember(testName, testPin);

    await repository.AddAsync(familyMember);

    var newFamilyMember = (await repository.ListAsync())
                    .FirstOrDefault();

    newFamilyMember.ShouldNotBeNull();
    testName.ShouldBe(newFamilyMember.Name);
    testPin.ShouldBe(newFamilyMember.Pin);
    newFamilyMember.Id.Value.ShouldBeGreaterThan(0);
  }
}
