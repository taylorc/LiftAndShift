using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.Core.WorkWeightAggregate.Specifications;

public class WorkWeightByFamilyMemberIdAndLiftSpec : Specification<WorkWeight>
{
  public WorkWeightByFamilyMemberIdAndLiftSpec(FamilyMemberId familyMemberId, Lift lift) =>
    Query
        .Where(workWeight => workWeight.FamilyMemberId == familyMemberId && workWeight.Lift == lift);
}
