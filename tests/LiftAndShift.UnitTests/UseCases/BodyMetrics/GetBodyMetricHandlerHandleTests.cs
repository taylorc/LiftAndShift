using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.BodyMetrics;

public class GetBodyMetricHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly BodyMetricId _testBodyMetricId = BodyMetricId.From(1);
  private readonly IReadRepository<BodyMetric> _repository = Substitute.For<IReadRepository<BodyMetric>>();
  private readonly GetBodyMetricHandler _handler;

  public GetBodyMetricHandlerHandleTests()
  {
    _handler = new GetBodyMetricHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingBodyMetric()
  {
    var result = await _handler.Handle(new GetBodyMetricQuery(_testFamilyMemberId, _testBodyMetricId), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsBodyMetricGivenExisting()
  {
    var bodyMetric = new BodyMetric(_testFamilyMemberId, new DateOnly(2026, 10, 12), WeightKg.From(82.5m));

    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<BodyMetric>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<BodyMetric?>(bodyMetric));

    var result = await _handler.Handle(new GetBodyMetricQuery(_testFamilyMemberId, _testBodyMetricId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.FamilyMemberId.ShouldBe(_testFamilyMemberId);
    result.Value.RecordedOn.ShouldBe(new DateOnly(2026, 10, 12));
    result.Value.BodyWeightKg.ShouldBe(WeightKg.From(82.5m));
  }
}
