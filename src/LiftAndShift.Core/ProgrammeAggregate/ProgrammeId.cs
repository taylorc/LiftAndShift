using Vogen;

namespace LiftAndShift.Core.ProgrammeAggregate;

[ValueObject<int>]
public readonly partial struct ProgrammeId
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("ProgrammeId must be positive.");
}
