using Vogen;

namespace LiftAndShift.Core.WorkoutSessionAggregate;

[ValueObject<int>]
public readonly partial struct LoggedSetId
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("LoggedSetId must be positive.");
}
