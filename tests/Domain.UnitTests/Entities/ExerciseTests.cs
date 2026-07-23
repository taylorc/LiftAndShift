using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class ExerciseTests
{
    [Test]
    public void NameShouldDefaultToEmptyString()
    {
        var exercise = new Exercise();

        exercise.Name.ShouldBe(string.Empty);
    }

    [Test]
    public void IsActiveShouldDefaultToTrue()
    {
        var exercise = new Exercise();

        exercise.IsActive.ShouldBeTrue();
    }

    [Test]
    public void IsCustomShouldDefaultToFalse()
    {
        var exercise = new Exercise();

        exercise.IsCustom.ShouldBeFalse();
    }

    [Test]
    public void OptionalPropertiesShouldDefaultToNull()
    {
        var exercise = new Exercise();

        exercise.Description.ShouldBeNull();
        exercise.CreatedByUserId.ShouldBeNull();
    }

    [Test]
    public void ShouldRetainAssignedPropertyValues()
    {
        var exercise = new Exercise
        {
            Name = "Squat",
            Description = "Barbell back squat",
            MuscleGroup = MuscleGroup.Legs,
            EquipmentType = EquipmentType.Barbell,
            MovementPattern = MovementPattern.Squat,
            IsCustom = true,
            CreatedByUserId = "user-1",
            IsActive = false
        };

        exercise.Name.ShouldBe("Squat");
        exercise.Description.ShouldBe("Barbell back squat");
        exercise.MuscleGroup.ShouldBe(MuscleGroup.Legs);
        exercise.EquipmentType.ShouldBe(EquipmentType.Barbell);
        exercise.MovementPattern.ShouldBe(MovementPattern.Squat);
        exercise.IsCustom.ShouldBeTrue();
        exercise.CreatedByUserId.ShouldBe("user-1");
        exercise.IsActive.ShouldBeFalse();
    }
}
