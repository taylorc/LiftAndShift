using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.Core.WorkWeightAggregate.Specifications;

/// <summary>
/// The lifts a family member has already Ramped (i.e. has an established WorkWeight for),
/// projected directly to Lift so callers checking "has this lift been Ramped?" don't need
/// to materialize a WorkWeight entity per lift, or per family member.
/// </summary>
public class RampedLiftsByFamilyMemberIdSpec : Specification<WorkWeight, Lift>
{
  public RampedLiftsByFamilyMemberIdSpec(FamilyMemberId familyMemberId) =>
    Query
        .Where(workWeight => workWeight.FamilyMemberId == familyMemberId)
        .Select(workWeight => workWeight.Lift);
}
