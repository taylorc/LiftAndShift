using Ardalis.Specification;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.UnitTests.UseCases.WorkoutSessions;

public class LogWorkoutSessionHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly DateOnly _testPerformedOn = new(2026, 9, 21);
  private readonly IRepository<WorkoutSession> _workoutSessionRepository = Substitute.For<IRepository<WorkoutSession>>();
  private readonly IReadRepository<LiftAndShift.Core.ProgrammeAggregate.Programme> _programmeRepository = Substitute.For<IReadRepository<LiftAndShift.Core.ProgrammeAggregate.Programme>>();
  private readonly IRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight> _workWeightRepository = Substitute.For<IRepository<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>();
  private readonly LogWorkoutSessionHandler _handler;

  public LogWorkoutSessionHandlerHandleTests()
  {
    _handler = new LogWorkoutSessionHandler(_workoutSessionRepository, _programmeRepository, _workWeightRepository);
  }

  // RampedWorkWeight(lift) below ramps every lift to finalSetNumber 3, so these must match each
  // lift's RampWeightForSet(3): Squat/Press 30kg (opening 20 + 2*5), Deadlift 90kg (opening 70 + 2*10).
  private static IReadOnlyList<LoggedLiftInput> ValidWorkoutALoggedLifts(int squatFinalRep = 5) =>
  [
    new(Lift.Squat, WeightKg.From(30), [5, 5, squatFinalRep]),
    new(Lift.Press, WeightKg.From(30), [5, 5, 5]),
    new(Lift.Deadlift, WeightKg.From(90), [5])
  ];

  private void GivenProgrammeAtPhase1() =>
    _programmeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<LiftAndShift.Core.ProgrammeAggregate.Programme>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<LiftAndShift.Core.ProgrammeAggregate.Programme?>(
        new LiftAndShift.Core.ProgrammeAggregate.Programme(_testFamilyMemberId, TrainingPhase.From(1))));

  private void GivenRampedWorkWeights(params LiftAndShift.Core.WorkWeightAggregate.WorkWeight[] workWeights) =>
    _workWeightRepository.ListAsync(Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<List<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>([.. workWeights]));

  private LiftAndShift.Core.WorkWeightAggregate.WorkWeight RampedWorkWeight(Lift lift) =>
    LiftAndShift.Core.WorkWeightAggregate.WorkWeight.CompleteRamp(_testFamilyMemberId, lift, 3);

  private void GivenAllWorkoutALiftsAreRamped() =>
    GivenRampedWorkWeights(RampedWorkWeight(Lift.Squat), RampedWorkWeight(Lift.Press), RampedWorkWeight(Lift.Deadlift), RampedWorkWeight(Lift.BenchPress));

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
    GivenRampedWorkWeights(RampedWorkWeight(Lift.Squat), RampedWorkWeight(Lift.Press)); // Deadlift not Ramped

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
      Arg.Any<ISpecification<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ReturnsInvalidGivenWrongLiftsForTheWorkout()
  {
    GivenProgrammeAtPhase1();
    GivenAllWorkoutALiftsAreRamped();

    IReadOnlyList<LoggedLiftInput> wrongLifts =
    [
      new(Lift.Squat, WeightKg.From(30), [5, 5, 5]),
      new(Lift.Press, WeightKg.From(30), [5, 5, 5]),
      new(Lift.BenchPress, WeightKg.From(30), [5, 5, 5]) // Bench Press doesn't belong to Workout A
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

  [Fact]
  public async Task SuccessfulLiftsProgressAndAreReportedInLiftOutcomes()
  {
    GivenProgrammeAtPhase1();
    GivenAllWorkoutALiftsAreRamped(); // Squat starts at 30kg

    var result = await _handler.Handle(
      new LogWorkoutSessionCommand(_testFamilyMemberId, Workout.A, _testPerformedOn, ValidWorkoutALoggedLifts()),
      CancellationToken.None);

    var squatOutcome = result.Value.LiftOutcomes.Single(o => o.Lift == Lift.Squat);
    squatOutcome.Successful.ShouldBeTrue();
    squatOutcome.Deloaded.ShouldBeFalse();
    squatOutcome.NewWeightKg.ShouldBe(WeightKg.From(35)); // 30 + Squat's 5kg progression increment
    await _workWeightRepository.Received(3).UpdateAsync(Arg.Any<LiftAndShift.Core.WorkWeightAggregate.WorkWeight>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task AFailedLiftDoesNotProgressAndIsReportedAsUnsuccessful()
  {
    GivenProgrammeAtPhase1();
    GivenAllWorkoutALiftsAreRamped(); // Squat starts at 30kg

    var result = await _handler.Handle(
      new LogWorkoutSessionCommand(_testFamilyMemberId, Workout.A, _testPerformedOn, ValidWorkoutALoggedLifts(squatFinalRep: 3)),
      CancellationToken.None);

    var squatOutcome = result.Value.LiftOutcomes.Single(o => o.Lift == Lift.Squat);
    squatOutcome.Successful.ShouldBeFalse();
    squatOutcome.Deloaded.ShouldBeFalse();
    squatOutcome.NewWeightKg.ShouldBe(WeightKg.From(30)); // unchanged on the first failure
  }

  [Fact]
  public async Task ASecondConsecutiveFailureDeloadsTheLift()
  {
    GivenProgrammeAtPhase1();
    var alreadyFailingSquat = RampedWorkWeight(Lift.Squat); // 30kg
    alreadyFailingSquat.RecordFailure(); // streak = 1 already
    GivenRampedWorkWeights(alreadyFailingSquat, RampedWorkWeight(Lift.Press), RampedWorkWeight(Lift.Deadlift));

    var result = await _handler.Handle(
      new LogWorkoutSessionCommand(_testFamilyMemberId, Workout.A, _testPerformedOn, ValidWorkoutALoggedLifts(squatFinalRep: 3)),
      CancellationToken.None);

    var squatOutcome = result.Value.LiftOutcomes.Single(o => o.Lift == Lift.Squat);
    squatOutcome.Successful.ShouldBeFalse();
    squatOutcome.Deloaded.ShouldBeTrue();
    squatOutcome.NewWeightKg.ShouldBe(WeightKg.From(27)); // floor(30 * 0.9)
  }
}
