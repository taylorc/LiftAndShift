using LiftAndShift.Core.ContributorAggregate;

namespace LiftAndShift.IntegrationTests.Data;

public class EfRepositoryDeleteTests : BaseEfRepoTestFixture
{
  [Fact]
  public async Task DeletesItemAfterAddingIt()
  {
    // add a Contributor
    var repository = GetRepository();
    var initialName = ContributorName.From(Guid.NewGuid().ToString());
    var Contributor = new Contributor(initialName);
    await repository.AddAsync(Contributor);

    // delete the item
    await repository.DeleteAsync(Contributor);

    // verify it's no longer there
    (await repository.ListAsync()).ShouldNotContain(Contributor => Contributor.Name == initialName);
  }
}
