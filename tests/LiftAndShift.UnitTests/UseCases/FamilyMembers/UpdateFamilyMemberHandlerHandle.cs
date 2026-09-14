namespace LiftAndShift.UnitTests.UseCases.FamilyMembers;

public class UpdateFamilyMemberHandlerHandle
{
  private readonly IRepository<FamilyMember> _repository = Substitute.For<IRepository<FamilyMember>>();
  private readonly UpdateFamilyMemberHandler _handler;

  public UpdateFamilyMemberHandlerHandle()
  {
    _handler = new UpdateFamilyMemberHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingFamilyMember()
  {
    var command = new UpdateFamilyMemberCommand(FamilyMemberId.From(9999), FamilyMemberName.From("Ada"), Pin.From("1234"));
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }
}
