using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Workouts.Commands;

public class LogWorkoutCommandValidatorTests
{
    private LogWorkoutCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new LogWorkoutCommandValidator();
    }

    [Test]
    public void ShouldPassValidation_WhenCommandIsValid()
    {
        var result = _validator.Validate(new LogWorkoutCommand());

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldFailValidation_WhenDateIsDefault()
    {
        var result = _validator.Validate(new LogWorkoutCommand { Date = default });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(LogWorkoutCommand.Date));
    }

    [Test]
    public void ShouldFailValidation_WhenExercisesIsNull()
    {
        var result = _validator.Validate(new LogWorkoutCommand { Exercises = null! });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(LogWorkoutCommand.Exercises));
    }
}
