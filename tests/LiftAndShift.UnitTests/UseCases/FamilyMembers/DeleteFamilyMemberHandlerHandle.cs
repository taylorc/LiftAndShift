namespace LiftAndShift.UnitTests.UseCases.FamilyMembers;

public class DeleteFamilyMemberHandlerHandle
{
  private readonly IRepository<FamilyMember> _repository = Substitute.For<IRepository<FamilyMember>>();
  private readonly DeleteFamilyMemberHandler _handler;

  public DeleteFamilyMemberHandlerHandle()
  {
    _handler = new DeleteFamilyMemberHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingFamilyMember()
  {
    var result = await _handler.Handle(new DeleteFamilyMemberCommand(FamilyMemberId.From(9999)), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }
}
