using FluentValidation;

namespace LiftAndShift.Application.Onboarding.Commands.SaveUserOnboarding;

public class SaveUserOnboardingCommandValidator : AbstractValidator<SaveUserOnboardingCommand>
{
    private static readonly string[] ValidUnits = ["Lbs", "Kgs"];
    private static readonly string[] ValidAlternatingLifts = ["PowerClean", "PendlayRow"];

    public SaveUserOnboardingCommandValidator()
    {
        RuleFor(x => x.PreferredUnit)
            .Must(u => ValidUnits.Contains(u))
            .WithMessage("PreferredUnit must be 'Lbs' or 'Kgs'.");

        RuleFor(x => x.BodyWeight)
            .GreaterThan(0)
            .WithMessage("Body weight must be greater than zero.");

        RuleFor(x => x.AlternatingLift)
            .Must(l => ValidAlternatingLifts.Contains(l))
            .WithMessage("AlternatingLift must be 'PowerClean' or 'PendlayRow'.");

        RuleFor(x => x.SquatStartingWeight).GreaterThan(0);
        RuleFor(x => x.BenchPressStartingWeight).GreaterThan(0);
        RuleFor(x => x.OverheadPressStartingWeight).GreaterThan(0);
        RuleFor(x => x.DeadliftStartingWeight).GreaterThan(0);
        RuleFor(x => x.AlternatingLiftStartingWeight).GreaterThan(0);
    }
}
