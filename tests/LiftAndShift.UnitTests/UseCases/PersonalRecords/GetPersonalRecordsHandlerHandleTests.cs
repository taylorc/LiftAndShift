using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.PersonalRecords;

public class GetPersonalRecordsHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly IReadRepository<FamilyMember> _familyMemberRepository = Substitute.For<IReadRepository<FamilyMember>>();
  private readonly IReadRepository<WorkoutSession> _workoutSessionRepository = Substitute.For<IReadRepository<WorkoutSession>>();
  private readonly GetPersonalRecordsHandler _handler;

  public GetPersonalRecordsHandlerHandleTests()
  {
    _handler = new GetPersonalRecordsHandler(_familyMemberRepository, _workoutSessionRepository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenNonexistentFamilyMember()
  {
    var result = await _handler.Handle(new GetPersonalRecordsQuery(_testFamilyMemberId), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsEmptyListGivenFamilyMemberWithNoSessions()
  {
    _familyMemberRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(new FamilyMember(FamilyMemberName.From("Ada"), Pin.From("1234"))));
    _workoutSessionRepository.ListAsync(Arg.Any<ISpecification<WorkoutSession>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<List<WorkoutSession>>([]));

    var result = await _handler.Handle(new GetPersonalRecordsQuery(_testFamilyMemberId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBeEmpty();
  }

  [Fact]
  public async Task ReturnsRecordsComputedFromLoggedSessions()
  {
    _familyMemberRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(new FamilyMember(FamilyMemberName.From("Ada"), Pin.From("1234"))));

    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), new DateOnly(2026, 1, 1),
      [
        new LoggedLiftInput(Lift.Squat, WeightKg.From(60), [5, 5, 5]),
        new LoggedLiftInput(Lift.Press, WeightKg.From(20), [5, 5, 5]),
        new LoggedLiftInput(Lift.Deadlift, WeightKg.From(70), [5])
      ]);
    _workoutSessionRepository.ListAsync(Arg.Any<ISpecification<WorkoutSession>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<List<WorkoutSession>>([session]));

    var result = await _handler.Handle(new GetPersonalRecordsQuery(_testFamilyMemberId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldContain(r => r.Lift == Lift.Squat && r.WeightKg == WeightKg.From(60));
  }
}
