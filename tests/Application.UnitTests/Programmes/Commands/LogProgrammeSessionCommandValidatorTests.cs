using LiftAndShift.Application.Programmes.Commands.LogProgrammeSession;
using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using LiftAndShift.Domain.Enums;
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

    private static LogProgrammeSessionCommand CommandWith(params LogWorkoutSetDto[] sets) => new()
    {
        UserProgrammeId = 1,
        ProgrammeSessionId = 1,
        Exercises = { new LogWorkoutExerciseDto { ExerciseId = 1, OrderIndex = 0, Sets = sets.ToList() } }
    };

    private static LogWorkoutSetDto WorkingSet(int? completedReps, bool isCompleted) => new()
    {
        SetNumber = 1,
        SetType = SetType.WorkingSet,
        WeightKg = 100,
        Reps = 5,
        CompletedReps = completedReps,
        IsCompleted = isCompleted
    };

    private static LogWorkoutSetDto WarmupSet(int? completedReps, bool isCompleted) => new()
    {
        SetNumber = 1,
        SetType = SetType.Warmup,
        WeightKg = 40,
        Reps = 5,
        CompletedReps = completedReps,
        IsCompleted = isCompleted
    };

    [Test]
    public void ShouldPassValidation_WhenEveryWorkingSetHasCompletedRepsAndIsDone()
    {
        var result = _validator.Validate(CommandWith(
            WarmupSet(completedReps: null, isCompleted: false),
            WorkingSet(completedReps: 5, isCompleted: true)));

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldFailValidation_WhenAWorkingSetHasNoCompletedReps()
    {
        var result = _validator.Validate(CommandWith(
            WorkingSet(completedReps: null, isCompleted: false)));

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldFailValidation_WhenCompletedRepsIsFilledButSetIsNotMarkedDone()
    {
        var result = _validator.Validate(CommandWith(
            WorkingSet(completedReps: 5, isCompleted: false)));

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldFailValidation_WhenAFilledWarmupSetIsNotMarkedDone()
    {
        var result = _validator.Validate(CommandWith(
            WarmupSet(completedReps: 5, isCompleted: false),
            WorkingSet(completedReps: 5, isCompleted: true)));

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldPassValidation_WhenAWarmupSetIsLeftBlank()
    {
        var result = _validator.Validate(CommandWith(
            WarmupSet(completedReps: null, isCompleted: false),
            WorkingSet(completedReps: 5, isCompleted: true)));

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
