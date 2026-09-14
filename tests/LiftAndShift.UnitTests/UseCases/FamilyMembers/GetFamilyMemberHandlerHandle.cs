using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.FamilyMembers;

public class GetFamilyMemberHandlerHandle
{
  private readonly FamilyMemberName _testName = FamilyMemberName.From("Ada");
  private readonly Pin _testPin = Pin.From("1234");
  private readonly IReadRepository<FamilyMember> _repository = Substitute.For<IReadRepository<FamilyMember>>();
  private readonly GetFamilyMemberHandler _handler;

  public GetFamilyMemberHandlerHandle()
  {
    _handler = new GetFamilyMemberHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingFamilyMember()
  {
    var result = await _handler.Handle(new GetFamilyMemberQuery(FamilyMemberId.From(9999)), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsFamilyMemberGivenExistingId()
  {
    var familyMember = new FamilyMember(_testName, _testPin);
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(familyMember));

    var result = await _handler.Handle(new GetFamilyMemberQuery(familyMember.Id), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Name.ShouldBe(_testName);
    result.Value.Pin.ShouldBe(_testPin);
  }
}
