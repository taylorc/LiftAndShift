using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.WorkoutSessions;

public class GetWorkoutSessionHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly WorkoutSessionId _testWorkoutSessionId = WorkoutSessionId.From(1);
  private readonly IReadRepository<WorkoutSession> _repository = Substitute.For<IReadRepository<WorkoutSession>>();
  private readonly GetWorkoutSessionHandler _handler;

  public GetWorkoutSessionHandlerHandleTests()
  {
    _handler = new GetWorkoutSessionHandler(_repository);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenMissingWorkoutSession()
  {
    var result = await _handler.Handle(new GetWorkoutSessionQuery(_testFamilyMemberId, _testWorkoutSessionId), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsWorkoutSessionGivenExisting()
  {
    var session = WorkoutSession.Log(
      _testFamilyMemberId, Workout.A, TrainingPhase.From(1), new DateOnly(2026, 9, 21),
      [
        new LoggedLiftInput(Lift.Squat, WeightKg.From(30), [5, 5, 5]),
        new LoggedLiftInput(Lift.Press, WeightKg.From(25), [5, 5, 5]),
        new LoggedLiftInput(Lift.Deadlift, WeightKg.From(90), [5])
      ]);

    _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<WorkoutSession>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<WorkoutSession?>(session));

    var result = await _handler.Handle(new GetWorkoutSessionQuery(_testFamilyMemberId, _testWorkoutSessionId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.FamilyMemberId.ShouldBe(_testFamilyMemberId);
    result.Value.Workout.ShouldBe(Workout.A);
    result.Value.LoggedSets.Count.ShouldBe(7);
  }
}
