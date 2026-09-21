using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.WorkWeights;

public class GetWarmUpSetsHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly IReadRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight> _repository = Substitute.For<IReadRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>();
  private readonly GetWarmUpSetsHandler _handler;

  public GetWarmUpSetsHandlerHandleTests()
  {
    _handler = new GetWarmUpSetsHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingWorkWeight()
  {
    var result = await _handler.Handle(new GetWarmUpSetsQuery(_testFamilyMemberId, Lift.Squat), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsWarmUpSetsGivenExistingWorkWeight()
  {
    var workWeight = LiftAndShift.Core.WorkWeightAggregate.WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, 3); // 30kg
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.WorkWeightAggregate.WorkWeight?>(workWeight));

    var result = await _handler.Handle(new GetWarmUpSetsQuery(_testFamilyMemberId, Lift.Squat), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Count.ShouldBe(4);
    result.Value[0].WeightKg.ShouldBe(WeightKg.From(20));
  }
}
