namespace LiftAndShift.Core.FamilyMemberAggregate.Specifications;

public class FamilyMemberByIdSpec : Specification<FamilyMember>
{
  public FamilyMemberByIdSpec(FamilyMemberId familyMemberId) =>
    Query
        .Where(familyMember => familyMember.Id == familyMemberId);
}
