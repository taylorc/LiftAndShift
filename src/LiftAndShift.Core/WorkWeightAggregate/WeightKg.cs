using Vogen;

namespace LiftAndShift.Core.WorkWeightAggregate;

[ValueObject<decimal>(conversions: Conversions.SystemTextJson)]
public readonly partial struct WeightKg
{
  private static Validation Validate(decimal value)
      => value > 0 ? Validation.Ok : Validation.Invalid("WeightKg must be positive.");
}
