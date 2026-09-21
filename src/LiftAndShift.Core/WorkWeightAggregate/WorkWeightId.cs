using Vogen;

namespace LiftAndShift.Core.WorkWeightAggregate;

[ValueObject<int>]
public readonly partial struct WorkWeightId
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("WorkWeightId must be positive.");
}
