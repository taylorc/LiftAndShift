using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkoutSessionAggregate.Specifications;

namespace LiftAndShift.UseCases.WorkoutSessions.Get;

public class GetWorkoutSessionHandler(IReadRepository<WorkoutSession> _repository)
  : IQueryHandler<GetWorkoutSessionQuery, Result<WorkoutSessionDto>>
{
  public async ValueTask<Result<WorkoutSessionDto>> Handle(GetWorkoutSessionQuery request, CancellationToken cancellationToken)
  {
    var spec = new WorkoutSessionByFamilyMemberIdAndIdSpec(request.FamilyMemberId, request.WorkoutSessionId);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null) return Result.NotFound();

    // LiftOutcomes reflects what changed to Work Weight *at the moment a session was logged*; that's
    // not something we retroactively reconstruct for a historical session, so it's always empty here.
    return new WorkoutSessionDto(
      entity.Id,
      entity.FamilyMemberId,
      entity.Workout,
      entity.TrainingPhase,
      entity.PerformedOn,
      entity.LoggedSets
        .Select(s => new LoggedSetDto(s.Id, s.Lift, s.WeightKg, s.SetNumber, s.RepsAchieved))
        .ToList(),
      []);
  }
}
