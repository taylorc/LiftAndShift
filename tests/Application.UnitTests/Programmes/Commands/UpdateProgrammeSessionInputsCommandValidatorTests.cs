using LiftAndShift.Application.Programmes.Commands.UpdateProgrammeSessionInputs;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class UpdateProgrammeSessionInputsCommandValidatorTests
{
    private UpdateProgrammeSessionInputsCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateProgrammeSessionInputsCommandValidator();
    }

    [Test]
    public void ShouldPassValidation_WhenCommandIsValid()
    {
        var result = _validator.Validate(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = 1,
            ProgrammeSessionId = 1,
            LiftProgression = new() { ["Squat"] = 100m },
            ConsecutiveFailures = new() { ["Squat"] = 0 },
        });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldFailValidation_WhenNeitherOverrideIsProvided()
    {
        var result = _validator.Validate(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = 1,
            ProgrammeSessionId = 1,
        });

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldFailValidation_WhenLiftProgressionWeightIsZeroOrNegative()
    {
        var result = _validator.Validate(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = 1,
            ProgrammeSessionId = 1,
            LiftProgression = new() { ["Squat"] = 0m },
        });

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldFailValidation_WhenConsecutiveFailuresIsNegative()
    {
        var result = _validator.Validate(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = 1,
            ProgrammeSessionId = 1,
            ConsecutiveFailures = new() { ["Squat"] = -1 },
        });

        result.IsValid.ShouldBeFalse();
    }
}
