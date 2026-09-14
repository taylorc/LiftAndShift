using Vogen;

namespace LiftAndShift.Core.FamilyMemberAggregate;

[ValueObject<int>]
public readonly partial struct FamilyMemberId
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("FamilyMemberId must be positive.");
}
