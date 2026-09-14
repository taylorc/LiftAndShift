using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.Core.ProgrammeAggregate.Specifications;

public class ProgrammeByFamilyMemberIdSpec : Specification<Programme>
{
  public ProgrammeByFamilyMemberIdSpec(FamilyMemberId familyMemberId) =>
    Query
        .Where(programme => programme.FamilyMemberId == familyMemberId);
}
