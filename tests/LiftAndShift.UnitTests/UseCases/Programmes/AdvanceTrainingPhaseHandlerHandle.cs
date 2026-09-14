using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.Programmes;

public class AdvanceTrainingPhaseHandlerHandle
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly IRepository<LiftAndShift.Core.ProgrammeAggregate.Programme> _repository = Substitute.For<IRepository<LiftAndShift.Core.ProgrammeAggregate.Programme>>();
  private readonly AdvanceTrainingPhaseHandler _handler;

  public AdvanceTrainingPhaseHandlerHandle()
  {
    _handler = new AdvanceTrainingPhaseHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingProgramme()
  {
    var result = await _handler.Handle(new AdvanceTrainingPhaseCommand(_testFamilyMemberId), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task AdvancesPhaseGivenExistingProgramme()
  {
    var programme = new LiftAndShift.Core.ProgrammeAggregate.Programme(_testFamilyMemberId, TrainingPhase.From(1));
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.ProgrammeAggregate.Programme>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.ProgrammeAggregate.Programme?>(programme));

    var result = await _handler.Handle(new AdvanceTrainingPhaseCommand(_testFamilyMemberId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.TrainingPhase.ShouldBe(TrainingPhase.From(2));
  }

  [Fact]
  public async Task ReturnsInvalidGivenProgrammeAlreadyAtFinalPhase()
  {
    var programme = new LiftAndShift.Core.ProgrammeAggregate.Programme(_testFamilyMemberId, TrainingPhase.From(3));
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.ProgrammeAggregate.Programme>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.ProgrammeAggregate.Programme?>(programme));

    var result = await _handler.Handle(new AdvanceTrainingPhaseCommand(_testFamilyMemberId), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.Invalid);
  }
}
