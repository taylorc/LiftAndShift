using Ardalis.Specification;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.UnitTests.UseCases.WorkoutSessions;

public class LogWorkoutSessionHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly DateOnly _testPerformedOn = new(2026, 9, 21);
  private readonly IRepository<WorkoutSession> _workoutSessionRepository = Substitute.For<IRepository<WorkoutSession>>();
  private readonly IReadRepository<LiftAndShift.Core.ProgrammeAggregate.Programme> _programmeRepository = Substitute.For<IReadRepository<LiftAndShift.Core.ProgrammeAggregate.Programme>>();
  private readonly IReadRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight> _workWeightRepository = Substitute.For<IReadRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>();
  private readonly LogWorkoutSessionHandler _handler;

  public LogWorkoutSessionHandlerHandleTests()
  {
    _handler = new LogWorkoutSessionHandler(_workoutSessionRepository, _programmeRepository, _workWeightRepository);
  }

  private static IReadOnlyList<LoggedLiftInput> ValidWorkoutALoggedLifts() =>
  [
    new(Lift.Squat, WeightKg.From(30), [5, 5, 5]),
    new(Lift.Press, WeightKg.From(25), [5, 5, 5]),
    new(Lift.Deadlift, WeightKg.From(90), [5])
  ];

  private void GivenProgrammeAtPhase1() =>
    _programmeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.ProgrammeAggregate.Programme>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.ProgrammeAggregate.Programme?>(
        new LiftAndShift.Core.ProgrammeAggregate.Programme(_testFamilyMemberId, TrainingPhase.From(1))));

  private void GivenRampedLifts(params Lift[] rampedLifts) =>
    _workWeightRepository.ListAsync(Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight, Lift>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<List<Lift>>([.. rampedLifts]));

  private void GivenAllWorkoutALiftsAreRamped() => GivenRampedLifts(Lift.Squat, Lift.Press, Lift.Deadlift, Lift.BenchPress);

  [Fact]
  public async Task ReturnsNotFoundGivenNoProgrammeForFamilyMember()
  {
    var result = await _handler.Handle(
      new LogWorkoutSessionCommand(_testFamilyMemberId, Workout.A, _testPerformedOn, ValidWorkoutALoggedLifts()),
      CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsInvalidGivenALiftHasNotBeenRamped()
  {
    GivenProgrammeAtPhase1();
    GivenRampedLifts(Lift.Squat, Lift.Press); // Deadlift not Ramped

    var result = await _handler.Handle(
      new LogWorkoutSessionCommand(_testFamilyMemberId, Workout.A, _testPerformedOn, ValidWorkoutALoggedLifts()),
      CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(Ardalis.Result.ResultStatus.Invalid);
    await _workoutSessionRepository.DidNotReceive().AddAsync(Arg.Any<WorkoutSession>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ChecksAllLoggedLiftsInASingleQuery()
  {
    GivenProgrammeAtPhase1();
    GivenAllWorkoutALiftsAreRamped();

    await _handler.Handle(
      new LogWorkoutSessionCommand(_testFamilyMemberId, Workout.A, _testPerformedOn, ValidWorkoutALoggedLifts()),
      CancellationToken.None);

    await _workWeightRepository.Received(1).ListAsync(
      Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight, Lift>>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ReturnsInvalidGivenWrongLiftsForTheWorkout()
  {
    GivenProgrammeAtPhase1();
    GivenAllWorkoutALiftsAreRamped();

    IReadOnlyList<LoggedLiftInput> wrongLifts =
    [
      new(Lift.Squat, WeightKg.From(30), [5, 5, 5]),
      new(Lift.Press, WeightKg.From(25), [5, 5, 5]),
      new(Lift.BenchPress, WeightKg.From(25), [5, 5, 5]) // Bench Press doesn't belong to Workout A
    ];

    var result = await _handler.Handle(
      new LogWorkoutSessionCommand(_testFamilyMemberId, Workout.A, _testPerformedOn, wrongLifts),
      CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(Ardalis.Result.ResultStatus.Invalid);
    await _workoutSessionRepository.DidNotReceive().AddAsync(Arg.Any<WorkoutSession>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CreatesWorkoutSessionGivenValidInputAndAllLiftsRamped()
  {
    GivenProgrammeAtPhase1();
    GivenAllWorkoutALiftsAreRamped();

    var result = await _handler.Handle(
      new LogWorkoutSessionCommand(_testFamilyMemberId, Workout.A, _testPerformedOn, ValidWorkoutALoggedLifts()),
      CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.FamilyMemberId.ShouldBe(_testFamilyMemberId);
    result.Value.Workout.ShouldBe(Workout.A);
    result.Value.TrainingPhase.ShouldBe(TrainingPhase.From(1));
    result.Value.LoggedSets.Count.ShouldBe(7);
    await _workoutSessionRepository.Received(1).AddAsync(Arg.Any<WorkoutSession>(), Arg.Any<CancellationToken>());
  }
}
