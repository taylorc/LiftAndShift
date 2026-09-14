using Vogen;

namespace LiftAndShift.Core.FamilyMemberAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct Pin
{
  private static Validation Validate(in string pin) =>
    pin is { Length: 4 } && pin.All(char.IsAsciiDigit)
      ? Validation.Ok
      : Validation.Invalid("Pin must be exactly 4 digits");
}
