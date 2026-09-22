using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.Workouts;
using FluentValidation;

namespace LiftAndShift.Web.Extensions;

public static class ValidatorExtensions
{
  public static IRuleBuilderOptions<T, string?> MustBeAKnownLift<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
    ruleBuilder
      .Must(name => Lift.TryFromName(name!, ignoreCase: true, out _))
      .WithMessage("Lift must be one of the known lifts.");

  public static IRuleBuilderOptions<T, string?> MustBeAKnownWorkout<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
    ruleBuilder
      .Must(name => Workout.TryFromName(name!, ignoreCase: true, out _))
      .WithMessage("Workout must be 'A' or 'B'.");
}
