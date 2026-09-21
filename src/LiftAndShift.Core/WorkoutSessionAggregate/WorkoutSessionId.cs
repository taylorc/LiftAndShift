using Vogen;

namespace LiftAndShift.Core.WorkoutSessionAggregate;

[ValueObject<int>]
public readonly partial struct WorkoutSessionId
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("WorkoutSessionId must be positive.");
}
