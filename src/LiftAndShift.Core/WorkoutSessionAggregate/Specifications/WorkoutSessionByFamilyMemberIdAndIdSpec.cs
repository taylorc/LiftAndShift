using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.Core.WorkoutSessionAggregate.Specifications;

public class WorkoutSessionByFamilyMemberIdAndIdSpec : Specification<WorkoutSession>
{
  public WorkoutSessionByFamilyMemberIdAndIdSpec(FamilyMemberId familyMemberId, WorkoutSessionId workoutSessionId) =>
    Query
        .Where(session => session.FamilyMemberId == familyMemberId && session.Id == workoutSessionId)
        .Include(session => session.LoggedSets);
}
