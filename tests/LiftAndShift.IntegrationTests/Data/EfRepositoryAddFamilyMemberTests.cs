using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.IntegrationTests.Data;

public class EfRepositoryAddFamilyMemberTests : BaseEfRepoTestFixture
{
  [Fact]
  public async Task AddsFamilyMemberAndSetsId()
  {
    var testName = FamilyMemberName.From("Ada");
    var testPin = Pin.From("1234");
    var repository = GetFamilyMemberRepository();
    var familyMember = new FamilyMember(testName, testPin);

    await repository.AddAsync(familyMember, TestContext.Current.CancellationToken);

    var newFamilyMember = (await repository.ListAsync(TestContext.Current.CancellationToken))
                    .FirstOrDefault();

    newFamilyMember.ShouldNotBeNull();
    testName.ShouldBe(newFamilyMember.Name);
    testPin.ShouldBe(newFamilyMember.Pin);
    newFamilyMember.Id.Value.ShouldBeGreaterThan(0);
  }
}
