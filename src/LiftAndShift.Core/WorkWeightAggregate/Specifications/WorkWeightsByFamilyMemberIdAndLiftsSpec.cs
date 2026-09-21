using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.Core.WorkWeightAggregate.Specifications;

/// <summary>
/// The WorkWeight entities a family member has established for a specific set of lifts, in one query.
/// Used both to check which of the requested lifts have (or haven't) been Ramped, and to fetch the
/// actual entities that need updating for progression/deload.
/// </summary>
public class WorkWeightsByFamilyMemberIdAndLiftsSpec : Specification<WorkWeight>
{
  public WorkWeightsByFamilyMemberIdAndLiftsSpec(FamilyMemberId familyMemberId, IEnumerable<Lift> lifts)
  {
    var liftList = lifts.ToList();
    Query
        .Where(workWeight => workWeight.FamilyMemberId == familyMemberId && liftList.Contains(workWeight.Lift));
  }
}
