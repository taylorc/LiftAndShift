namespace LiftAndShift.Web.FamilyMembers;

public record DeleteFamilyMemberRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}";
  public static string BuildRoute(int familyMemberId) => Route.Replace("{FamilyMemberId:int}", familyMemberId.ToString());

  public int FamilyMemberId { get; set; }
}
