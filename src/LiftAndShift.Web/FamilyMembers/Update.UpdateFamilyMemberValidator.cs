using LiftAndShift.Core.FamilyMemberAggregate;
using FastEndpoints;
using FluentValidation;

namespace LiftAndShift.Web.FamilyMembers;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class UpdateFamilyMemberValidator : Validator<UpdateFamilyMemberRequest>
{
  public UpdateFamilyMemberValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name is required.")
      .MinimumLength(2)
      .MaximumLength(FamilyMemberName.MaxLength);

    RuleFor(x => x.Pin)
      .NotEmpty()
      .WithMessage("Pin is required.")
      .Matches("^[0-9]{4}$")
      .WithMessage("Pin must be exactly 4 digits.");

    RuleFor(x => x.FamilyMemberId)
      .Must((args, familyMemberId) => args.Id == familyMemberId)
      .WithMessage("Route and body Ids must match; cannot update Id of an existing resource.");
  }
}
