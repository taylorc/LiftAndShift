using LiftAndShift.Application.Programmes.Commands.AdoptProgramme;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class AdoptProgrammeCommandValidatorTests
{
    private AdoptProgrammeCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new AdoptProgrammeCommandValidator();
    }

    [Test]
    public void ShouldPassValidation_WhenCommandIsValid()
    {
        var result = _validator.Validate(new AdoptProgrammeCommand());

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldFailValidation_WhenProgrammeTemplateIdIsEmpty()
    {
        var result = _validator.Validate(new AdoptProgrammeCommand { ProgrammeTemplateId = "" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AdoptProgrammeCommand.ProgrammeTemplateId));
    }
}
