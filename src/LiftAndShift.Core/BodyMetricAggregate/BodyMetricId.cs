using Vogen;

namespace LiftAndShift.Core.BodyMetricAggregate;

[ValueObject<int>]
public readonly partial struct BodyMetricId
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("BodyMetricId must be positive.");
}
