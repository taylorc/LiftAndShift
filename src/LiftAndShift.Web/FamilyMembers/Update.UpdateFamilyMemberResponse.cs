namespace LiftAndShift.Web.FamilyMembers;

public class UpdateFamilyMemberResponse(FamilyMemberRecord familyMember)
{
  public FamilyMemberRecord FamilyMember { get; set; } = familyMember;
}
