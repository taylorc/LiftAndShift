using System.ComponentModel.DataAnnotations;

namespace LiftAndShift.Web.FamilyMembers;

public class UpdateFamilyMemberRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}";
  public static string BuildRoute(int familyMemberId) => Route.Replace("{FamilyMemberId:int}", familyMemberId.ToString());

  public int FamilyMemberId { get; set; }

  [Required]
  public int Id { get; set; }
  [Required]
  public string? Name { get; set; }
  [Required]
  public string? Pin { get; set; }
}
