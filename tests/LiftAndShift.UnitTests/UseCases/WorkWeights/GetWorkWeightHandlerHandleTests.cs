using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.WorkWeights;

public class GetWorkWeightHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly IReadRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight> _repository = Substitute.For<IReadRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>();
  private readonly GetWorkWeightHandler _handler;

  public GetWorkWeightHandlerHandleTests()
  {
    _handler = new GetWorkWeightHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingWorkWeight()
  {
    var result = await _handler.Handle(new GetWorkWeightQuery(_testFamilyMemberId, Lift.Squat), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsWorkWeightGivenExisting()
  {
    var workWeight = LiftAndShift.Core.WorkWeightAggregate.WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, 3);
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.WorkWeightAggregate.WorkWeight?>(workWeight));

    var result = await _handler.Handle(new GetWorkWeightQuery(_testFamilyMemberId, Lift.Squat), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.FamilyMemberId.ShouldBe(_testFamilyMemberId);
    result.Value.Lift.ShouldBe(Lift.Squat);
    result.Value.WeightKg.ShouldBe(WeightKg.From(30));
  }
}
