using FluentValidation;
using LiftAndShift.Application.Onboarding.Commands.SaveUserOnboarding;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Onboarding.Commands;

public class SaveUserOnboardingCommandValidatorTests
{
    private SaveUserOnboardingCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new SaveUserOnboardingCommandValidator();
    }

    private static SaveUserOnboardingCommand ValidCommand() => new()
    {
        PreferredUnit = "Lbs",
        BodyWeight = 180m,
        AlternatingLift = "PowerClean",
        SquatStartingWeight = 135m,
        BenchPressStartingWeight = 95m,
        OverheadPressStartingWeight = 65m,
        DeadliftStartingWeight = 155m,
        AlternatingLiftStartingWeight = 95m
    };

    [Test]
    public void ShouldPassValidation_WhenCommandIsValid()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    [TestCase("Lbs")]
    [TestCase("Kgs")]
    public void ShouldPassValidation_ForAllValidUnits(string unit)
    {
        var result = _validator.Validate(ValidCommand() with { PreferredUnit = unit });
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(SaveUserOnboardingCommand.PreferredUnit));
    }

    [Test]
    [TestCase("kg")]
    [TestCase("lbs")]
    [TestCase("Stones")]
    [TestCase("")]
    public void ShouldFailValidation_ForInvalidUnit(string unit)
    {
        var result = _validator.Validate(ValidCommand() with { PreferredUnit = unit });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e =>
            e.PropertyName == nameof(SaveUserOnboardingCommand.PreferredUnit) &&
            e.ErrorMessage == "PreferredUnit must be 'Lbs' or 'Kgs'.");
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldFailValidation_WhenBodyWeightIsNotPositive(decimal weight)
    {
        var result = _validator.Validate(ValidCommand() with { BodyWeight = weight });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SaveUserOnboardingCommand.BodyWeight));
    }

    [Test]
    [TestCase("PowerClean")]
    [TestCase("PendlayRow")]
    public void ShouldPassValidation_ForAllValidAlternatingLifts(string lift)
    {
        var result = _validator.Validate(ValidCommand() with { AlternatingLift = lift });
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(SaveUserOnboardingCommand.AlternatingLift));
    }

    [Test]
    [TestCase("Barbell")]
    [TestCase("powerclean")]
    [TestCase("")]
    public void ShouldFailValidation_ForInvalidAlternatingLift(string lift)
    {
        var result = _validator.Validate(ValidCommand() with { AlternatingLift = lift });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e =>
            e.PropertyName == nameof(SaveUserOnboardingCommand.AlternatingLift) &&
            e.ErrorMessage == "AlternatingLift must be 'PowerClean' or 'PendlayRow'.");
    }

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void ShouldFailValidation_WhenSquatStartingWeightIsNotPositive(decimal weight)
    {
        var result = _validator.Validate(ValidCommand() with { SquatStartingWeight = weight });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SaveUserOnboardingCommand.SquatStartingWeight));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void ShouldFailValidation_WhenBenchPressStartingWeightIsNotPositive(decimal weight)
    {
        var result = _validator.Validate(ValidCommand() with { BenchPressStartingWeight = weight });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SaveUserOnboardingCommand.BenchPressStartingWeight));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void ShouldFailValidation_WhenOverheadPressStartingWeightIsNotPositive(decimal weight)
    {
        var result = _validator.Validate(ValidCommand() with { OverheadPressStartingWeight = weight });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SaveUserOnboardingCommand.OverheadPressStartingWeight));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void ShouldFailValidation_WhenDeadliftStartingWeightIsNotPositive(decimal weight)
    {
        var result = _validator.Validate(ValidCommand() with { DeadliftStartingWeight = weight });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SaveUserOnboardingCommand.DeadliftStartingWeight));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void ShouldFailValidation_WhenAlternatingLiftStartingWeightIsNotPositive(decimal weight)
    {
        var result = _validator.Validate(ValidCommand() with { AlternatingLiftStartingWeight = weight });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(SaveUserOnboardingCommand.AlternatingLiftStartingWeight));
    }
}
