using LiftAndShift.Application.Programmes.Commands.UpdateProgramme;
using LiftAndShift.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class UpdateProgrammeCommandValidatorTests
{
    private UpdateProgrammeCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateProgrammeCommandValidator();
    }

    [Test]
    public void ShouldPassValidation_WhenCommandIsValid()
    {
        var result = _validator.Validate(new UpdateProgrammeCommand
        {
            UserProgrammeId = 1,
            StartedAt = DateTimeOffset.UtcNow,
            Status = ProgrammeStatus.Active,
        });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldFailValidation_WhenNeitherStartedAtNorStatusIsProvided()
    {
        var result = _validator.Validate(new UpdateProgrammeCommand { UserProgrammeId = 1 });

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldFailValidation_WhenUserProgrammeIdIsNotPositive(int id)
    {
        var result = _validator.Validate(new UpdateProgrammeCommand { UserProgrammeId = id });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateProgrammeCommand.UserProgrammeId));
    }

    [Test]
    public void ShouldFailValidation_WhenStatusIsNotADefinedEnumValue()
    {
        var result = _validator.Validate(new UpdateProgrammeCommand { UserProgrammeId = 1, Status = (ProgrammeStatus)999 });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Status.Value");
    }
}
