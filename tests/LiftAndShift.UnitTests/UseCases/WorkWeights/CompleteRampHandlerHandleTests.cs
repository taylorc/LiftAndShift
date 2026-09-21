using Ardalis.Specification;
using NSubstitute.ExceptionExtensions;

namespace LiftAndShift.UnitTests.UseCases.WorkWeights;

public class CompleteRampHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly IRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight> _repository = Substitute.For<IRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>();
  private readonly IReadRepository<FamilyMember> _familyMemberRepository = Substitute.For<IReadRepository<FamilyMember>>();
  private readonly CompleteRampHandler _handler;

  public CompleteRampHandlerHandleTests()
  {
    _handler = new CompleteRampHandler(_repository, _familyMemberRepository);

    _familyMemberRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(new FamilyMember(FamilyMemberName.From("Ada"), Pin.From("1234"))));
  }

  [Fact]
  public async Task CreatesWorkWeightGivenLiftNotYetRamped()
  {
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.WorkWeightAggregate.WorkWeight?>(null));

    var result = await _handler.Handle(new CompleteRampCommand(_testFamilyMemberId, Lift.Squat, 3), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.WeightKg.ShouldBe(WeightKg.From(30));
    await _repository.Received(1).AddAsync(Arg.Any<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task FailsAndDoesNotAddGivenLiftAlreadyRamped()
  {
    var existing = LiftAndShift.Core.WorkWeightAggregate.WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, 3);
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.WorkWeightAggregate.WorkWeight?>(existing));

    var result = await _handler.Handle(new CompleteRampCommand(_testFamilyMemberId, Lift.Squat, 5), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    await _repository.DidNotReceive().AddAsync(Arg.Any<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ReturnsNotFoundGivenNonexistentFamilyMember()
  {
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.WorkWeightAggregate.WorkWeight?>(null));
    _familyMemberRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(null));

    var result = await _handler.Handle(new CompleteRampCommand(_testFamilyMemberId, Lift.Squat, 3), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
    await _repository.DidNotReceive().AddAsync(Arg.Any<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task TreatsInsertConflictAsAlreadyRamped()
  {
    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>(), Arg.Any<CancellationToken>())
      .Returns(
        Task.FromResult<LiftAndShift.Core.WorkWeightAggregate.WorkWeight?>(null),
        Task.FromResult<LiftAndShift.Core.WorkWeightAggregate.WorkWeight?>(
          LiftAndShift.Core.WorkWeightAggregate.WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, 3)));
    _repository.AddAsync(Arg.Any<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>(), Arg.Any<CancellationToken>())
      .Throws(new InvalidOperationException("simulated unique index violation"));

    var result = await _handler.Handle(new CompleteRampCommand(_testFamilyMemberId, Lift.Squat, 3), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(Ardalis.Result.ResultStatus.Invalid);
  }
}
