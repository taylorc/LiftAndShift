using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.Programmes;

public class GetProgrammeHandlerHandle
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly IReadRepository<LiftAndShift.Core.ProgrammeAggregate.Programme> _repository = Substitute.For<IReadRepository<LiftAndShift.Core.ProgrammeAggregate.Programme>>();
  private readonly GetProgrammeHandler _handler;

  public GetProgrammeHandlerHandle()
  {
    _handler = new GetProgrammeHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingProgramme()
  {
    var result = await _handler.Handle(new GetProgrammeQuery(_testFamilyMemberId), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsProgrammeGivenExistingFamilyMemberId()
  {
    var programme = new LiftAndShift.Core.ProgrammeAggregate.Programme(_testFamilyMemberId, TrainingPhase.From(1));
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.ProgrammeAggregate.Programme>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.ProgrammeAggregate.Programme?>(programme));

    var result = await _handler.Handle(new GetProgrammeQuery(_testFamilyMemberId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.FamilyMemberId.ShouldBe(_testFamilyMemberId);
    result.Value.TrainingPhase.ShouldBe(TrainingPhase.From(1));
  }
}
