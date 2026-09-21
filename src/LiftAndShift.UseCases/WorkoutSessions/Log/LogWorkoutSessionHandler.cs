using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.ProgrammeAggregate.Specifications;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.Core.WorkWeightAggregate.Specifications;

namespace LiftAndShift.UseCases.WorkoutSessions.Log;

public class LogWorkoutSessionHandler(
  IRepository<WorkoutSession> _workoutSessionRepository,
  IReadRepository<Programme> _programmeRepository,
  IRepository<WorkWeight> _workWeightRepository)
  : ICommandHandler<LogWorkoutSessionCommand, Result<WorkoutSessionDto>>
{
  public async ValueTask<Result<WorkoutSessionDto>> Handle(LogWorkoutSessionCommand command, CancellationToken ct)
  {
    var programme = await _programmeRepository.FirstOrDefaultAsync(
      new ProgrammeByFamilyMemberIdSpec(command.FamilyMemberId), ct);
    if (programme == null) return Result.NotFound();

    var requestedLifts = command.LoggedLifts.Select(l => l.Lift).ToList();
    var workWeights = await _workWeightRepository.ListAsync(
      new WorkWeightsByFamilyMemberIdAndLiftsSpec(command.FamilyMemberId, requestedLifts), ct);
    var workWeightsByLift = workWeights.ToDictionary(w => w.Lift);

    var unrampedLift = requestedLifts.FirstOrDefault(lift => !workWeightsByLift.ContainsKey(lift));
    if (unrampedLift != null)
    {
      return Result.Invalid(new ValidationError
      {
        Identifier = nameof(command.LoggedLifts),
        ErrorMessage = $"{unrampedLift.Name} has not been Ramped for this family member yet."
      });
    }

    // Progression/deload trusts the logged weight to decide what the *next* Work Weight should be,
    // so a session can only be logged at a lift's current Work Weight - otherwise a stray or stale
    // WeightKg (a typo, a warm-up submitted as a work set, ...) could silently move Work Weight to a
    // value nobody actually lifted.
    var mismatchedLift = command.LoggedLifts.FirstOrDefault(l => l.WeightKg != workWeightsByLift[l.Lift].WeightKg);
    if (mismatchedLift != null)
    {
      var currentWeight = workWeightsByLift[mismatchedLift.Lift].WeightKg;
      return Result.Invalid(new ValidationError
      {
        Identifier = nameof(command.LoggedLifts),
        ErrorMessage = $"{mismatchedLift.Lift.Name} must be logged at its current Work Weight ({currentWeight.Value}kg), not {mismatchedLift.WeightKg.Value}kg."
      });
    }

    WorkoutSession session;
    try
    {
      session = WorkoutSession.Log(
        command.FamilyMemberId, command.Workout, programme.TrainingPhase, command.PerformedOn, command.LoggedLifts);
    }
    catch (ArgumentException ex)
    {
      return Result.Invalid(new ValidationError
      {
        Identifier = nameof(command.LoggedLifts),
        ErrorMessage = ex.Message
      });
    }

    await _workoutSessionRepository.AddAsync(session, ct);

    var liftOutcomes = new List<LiftOutcomeDto>();
    foreach (var lift in requestedLifts)
    {
      var workWeight = workWeightsByLift[lift];
      bool successful = session.WasSuccessful(lift);
      bool deloaded = false;

      if (successful)
      {
        workWeight.RecordSuccess();
      }
      else
      {
        deloaded = workWeight.RecordFailure();
      }

      await _workWeightRepository.UpdateAsync(workWeight, ct);
      liftOutcomes.Add(new LiftOutcomeDto(lift, successful, workWeight.WeightKg, deloaded));
    }

    return ToDto(session, liftOutcomes);
  }

  private static WorkoutSessionDto ToDto(WorkoutSession session, IReadOnlyList<LiftOutcomeDto> liftOutcomes) =>
    new(
      session.Id,
      session.FamilyMemberId,
      session.Workout,
      session.TrainingPhase,
      session.PerformedOn,
      session.LoggedSets
        .Select(s => new LoggedSetDto(s.Id, s.Lift, s.WeightKg, s.SetNumber, s.RepsAchieved))
        .ToList(),
      liftOutcomes);
}
