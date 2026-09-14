using FastEndpoints;
using FluentValidation;

namespace LiftAndShift.Web.FamilyMembers;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class GetFamilyMemberValidator : Validator<GetFamilyMemberByIdRequest>
{
  public GetFamilyMemberValidator()
  {
    RuleFor(x => x.FamilyMemberId)
      .GreaterThan(0);
  }
}
