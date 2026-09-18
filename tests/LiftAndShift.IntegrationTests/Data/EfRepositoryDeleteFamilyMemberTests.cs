using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.IntegrationTests.Data;

public class EfRepositoryDeleteFamilyMemberTests : BaseEfRepoTestFixture
{
  [Fact]
  public async Task RemovesFamilyMember()
  {
    var repository = GetFamilyMemberRepository();
    var familyMember = new FamilyMember(FamilyMemberName.From("Ada"), Pin.From("1234"));
    await repository.AddAsync(familyMember, TestContext.Current.CancellationToken);

    await repository.DeleteAsync(familyMember, TestContext.Current.CancellationToken);

    var remaining = await repository.ListAsync(TestContext.Current.CancellationToken);
    remaining.ShouldNotContain(f => f.Id == familyMember.Id);
  }
}
