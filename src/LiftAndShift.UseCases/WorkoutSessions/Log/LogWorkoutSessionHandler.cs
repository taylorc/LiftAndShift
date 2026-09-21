using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.ProgrammeAggregate.Specifications;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.Core.WorkWeightAggregate.Specifications;

namespace LiftAndShift.UseCases.WorkoutSessions.Log;

public class LogWorkoutSessionHandler(
  IRepository<WorkoutSession> _workoutSessionRepository,
  IReadRepository<Programme> _programmeRepository,
  IReadRepository<WorkWeight> _workWeightRepository)
  : ICommandHandler<LogWorkoutSessionCommand, Result<WorkoutSessionDto>>
{
  public async ValueTask<Result<WorkoutSessionDto>> Handle(LogWorkoutSessionCommand command, CancellationToken ct)
  {
    var programme = await _programmeRepository.FirstOrDefaultAsync(
      new ProgrammeByFamilyMemberIdSpec(command.FamilyMemberId), ct);
    if (programme == null) return Result.NotFound();

    var rampedLifts = await _workWeightRepository.ListAsync(
      new RampedLiftsByFamilyMemberIdSpec(command.FamilyMemberId), ct);
    var unrampedLift = command.LoggedLifts.Select(l => l.Lift).FirstOrDefault(lift => !rampedLifts.Contains(lift));
    if (unrampedLift != null)
    {
      return Result.Invalid(new ValidationError
      {
        Identifier = nameof(command.LoggedLifts),
        ErrorMessage = $"{unrampedLift.Name} has not been Ramped for this family member yet."
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

    return ToDto(session);
  }

  private static WorkoutSessionDto ToDto(WorkoutSession session) =>
    new(
      session.Id,
      session.FamilyMemberId,
      session.Workout,
      session.TrainingPhase,
      session.PerformedOn,
      session.LoggedSets
        .Select(s => new LoggedSetDto(s.Id, s.Lift, s.WeightKg, s.SetNumber, s.RepsAchieved))
        .ToList());
}
