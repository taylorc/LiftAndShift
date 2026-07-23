using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class WorkoutSetTests
{
    [Test]
    public void SetTypeShouldDefaultToWorkingSet()
    {
        var set = new WorkoutSet();

        set.SetType.ShouldBe(SetType.WorkingSet);
    }

    [Test]
    public void IsCompletedShouldDefaultToFalse()
    {
        var set = new WorkoutSet();

        set.IsCompleted.ShouldBeFalse();
    }

    [Test]
    public void CompletedRepsShouldDefaultToNull()
    {
        var set = new WorkoutSet();

        set.CompletedReps.ShouldBeNull();
    }

    [Test]
    public void ShouldRetainAssignedPropertyValues()
    {
        var workoutExercise = new WorkoutExercise();

        var set = new WorkoutSet
        {
            WorkoutExerciseId = 1,
            WorkoutExercise = workoutExercise,
            SetNumber = 2,
            SetType = SetType.DropSet,
            WeightKg = 60m,
            Reps = 8,
            CompletedReps = 6,
            Notes = "Struggled on last rep",
            IsCompleted = true
        };

        set.WorkoutExerciseId.ShouldBe(1);
        set.WorkoutExercise.ShouldBe(workoutExercise);
        set.SetNumber.ShouldBe(2);
        set.SetType.ShouldBe(SetType.DropSet);
        set.WeightKg.ShouldBe(60m);
        set.Reps.ShouldBe(8);
        set.CompletedReps.ShouldBe(6);
        set.Notes.ShouldBe("Struggled on last rep");
        set.IsCompleted.ShouldBeTrue();
    }
}
