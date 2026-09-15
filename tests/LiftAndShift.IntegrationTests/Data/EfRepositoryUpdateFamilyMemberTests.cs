using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.IntegrationTests.Data;

public class EfRepositoryUpdateFamilyMemberTests : BaseEfRepoTestFixture
{
  [Fact]
  public async Task UpdatesNameAndPin()
  {
    var repository = GetFamilyMemberRepository();
    var familyMember = new FamilyMember(FamilyMemberName.From("Ada"), Pin.From("1234"));
    await repository.AddAsync(familyMember);

    var newName = FamilyMemberName.From("Grace");
    var newPin = Pin.From("4321");
    familyMember.UpdateName(newName);
    familyMember.UpdatePin(newPin);
    await repository.UpdateAsync(familyMember);

    var updated = await repository.GetByIdAsync(familyMember.Id);

    updated.ShouldNotBeNull();
    updated.Name.ShouldBe(newName);
    updated.Pin.ShouldBe(newPin);
  }
}
