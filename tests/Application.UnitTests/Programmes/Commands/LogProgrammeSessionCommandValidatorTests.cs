using LiftAndShift.Application.Programmes.Commands.LogProgrammeSession;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class LogProgrammeSessionCommandValidatorTests
{
    private LogProgrammeSessionCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new LogProgrammeSessionCommandValidator();
    }

    [Test]
    public void ShouldPassValidation_WhenIdsArePositive()
    {
        var result = _validator.Validate(new LogProgrammeSessionCommand { UserProgrammeId = 1, ProgrammeSessionId = 1 });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldFailValidation_WhenUserProgrammeIdIsNotPositive(int id)
    {
        var result = _validator.Validate(new LogProgrammeSessionCommand { UserProgrammeId = id, ProgrammeSessionId = 1 });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(LogProgrammeSessionCommand.UserProgrammeId));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldFailValidation_WhenProgrammeSessionIdIsNotPositive(int id)
    {
        var result = _validator.Validate(new LogProgrammeSessionCommand { UserProgrammeId = 1, ProgrammeSessionId = id });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(LogProgrammeSessionCommand.ProgrammeSessionId));
    }
}
