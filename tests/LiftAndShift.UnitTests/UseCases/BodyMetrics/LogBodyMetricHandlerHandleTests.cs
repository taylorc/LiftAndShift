using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.BodyMetrics;

public class LogBodyMetricHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly DateOnly _testRecordedOn = new(2026, 10, 12);
  private readonly IRepository<BodyMetric> _repository = Substitute.For<IRepository<BodyMetric>>();
  private readonly IReadRepository<FamilyMember> _familyMemberRepository = Substitute.For<IReadRepository<FamilyMember>>();
  private readonly LogBodyMetricHandler _handler;

  public LogBodyMetricHandlerHandleTests()
  {
    _handler = new LogBodyMetricHandler(_repository, _familyMemberRepository);
  }

  [Fact]
  public async Task ReturnsNotFoundAndDoesNotAddGivenMissingFamilyMember()
  {
    _familyMemberRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(null));

    var result = await _handler.Handle(
      new LogBodyMetricCommand(_testFamilyMemberId, _testRecordedOn, WeightKg.From(82.5m)), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
    await _repository.DidNotReceive().AddAsync(Arg.Any<BodyMetric>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task PersistsAndReturnsEntryGivenExistingFamilyMember()
  {
    _familyMemberRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(new FamilyMember(FamilyMemberName.From("Ada"), Pin.From("1234"))));

    var result = await _handler.Handle(
      new LogBodyMetricCommand(_testFamilyMemberId, _testRecordedOn, WeightKg.From(82.5m)), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.FamilyMemberId.ShouldBe(_testFamilyMemberId);
    result.Value.RecordedOn.ShouldBe(_testRecordedOn);
    result.Value.BodyWeightKg.ShouldBe(WeightKg.From(82.5m));
    await _repository.Received(1).AddAsync(
      Arg.Is<BodyMetric>(m => m.FamilyMemberId == _testFamilyMemberId && m.BodyWeightKg == WeightKg.From(82.5m)),
      Arg.Any<CancellationToken>());
  }
}
