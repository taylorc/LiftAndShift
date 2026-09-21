using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.WorkoutSessionAggregate;

namespace LiftAndShift.UseCases.WorkoutSessions.Get;

public record GetWorkoutSessionQuery(FamilyMemberId FamilyMemberId, WorkoutSessionId WorkoutSessionId) : IQuery<Result<WorkoutSessionDto>>;
