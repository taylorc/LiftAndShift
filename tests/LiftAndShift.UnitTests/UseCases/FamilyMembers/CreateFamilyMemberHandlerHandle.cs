namespace LiftAndShift.UnitTests.UseCases.FamilyMembers;

public class CreateFamilyMemberHandlerHandle
{
  private readonly FamilyMemberName _testName = FamilyMemberName.From("Ada");
  private readonly Pin _testPin = Pin.From("1234");
  private readonly IRepository<FamilyMember> _repository = Substitute.For<IRepository<FamilyMember>>();
  private readonly CreateFamilyMemberHandler _handler;

  public CreateFamilyMemberHandlerHandle()
  {
    _handler = new CreateFamilyMemberHandler(_repository);
  }

  private FamilyMember CreateFamilyMember()
  {
    return new FamilyMember(_testName, _testPin);
  }

  [Fact]
  public async Task ReturnsSuccessGivenValidNameAndPin()
  {
    _repository.AddAsync(Arg.Any<FamilyMember>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(CreateFamilyMember()));
    var result = await _handler.Handle(new CreateFamilyMemberCommand(_testName, _testPin), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
  }
}
