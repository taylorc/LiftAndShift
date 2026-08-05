using System;
using System.Collections.Generic;
using System.Text;
using LiftAndShift.Application.Exercises.Commands.CreateExercise;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Exercises.Commands;

public class CreateExerciseCommandValidatorTests
{
        private CreateExerciseCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new CreateExerciseCommandValidator();
        }

        [Test]
        public void ShouldPassValidation_WhenCommandIsValid()
        {
            var result = _validator.Validate(new CreateExerciseCommand { Name = "Push-ups" });

            result.IsValid.ShouldBeTrue();
        }

    [Test]
    public void ShouldFailValidation_WhenNameIsEmpty()
    {
        var result = _validator.Validate(new CreateExerciseCommand { Name = "" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateExerciseCommand.Name));
    }

    [Test]
    public void ShouldFailValidation_WhenNameIsGreaterThan200Characters()
    {
        var result = _validator.Validate(new CreateExerciseCommand { Name = new string('a', 201) });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateExerciseCommand.Name));
    }

    [Test]
    public void ShouldPassValidation_WhenNameIs200Characters()
    {
        var result = _validator.Validate(new CreateExerciseCommand { Name = new string('a', 200) });

        result.IsValid.ShouldBeTrue();
    }
}
