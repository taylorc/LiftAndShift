using FluentValidation;

namespace LiftAndShift.Web.FamilyMembers;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class DeleteFamilyMemberValidator : Validator<DeleteFamilyMemberRequest>
{
  public DeleteFamilyMemberValidator()
  {
    RuleFor(x => x.FamilyMemberId)
      .GreaterThan(0);
  }
}
