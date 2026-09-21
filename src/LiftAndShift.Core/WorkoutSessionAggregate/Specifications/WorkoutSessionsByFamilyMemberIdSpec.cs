using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.Core.WorkoutSessionAggregate.Specifications;

public class WorkoutSessionsByFamilyMemberIdSpec : Specification<WorkoutSession>
{
  public WorkoutSessionsByFamilyMemberIdSpec(FamilyMemberId familyMemberId) =>
    Query
        .Where(session => session.FamilyMemberId == familyMemberId)
        .Include(session => session.LoggedSets);
}
