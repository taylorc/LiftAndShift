using LiftAndShift.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class WorkoutExerciseTests
{
    [Test]
    public void SetsShouldBeEmptyByDefault()
    {
        var workoutExercise = new WorkoutExercise();

        workoutExercise.Sets.ShouldBeEmpty();
    }

    [Test]
    public void SetsShouldContainAddedSet()
    {
        var workoutExercise = new WorkoutExercise();
        var set = new WorkoutSet();

        workoutExercise.Sets.Add(set);

        workoutExercise.Sets.ShouldContain(set);
    }

    [Test]
    public void ShouldRetainAssignedPropertyValues()
    {
        var workoutSession = new WorkoutSession { UserId = "user-1" };
        var exercise = new Exercise { Name = "Squat" };

        var workoutExercise = new WorkoutExercise
        {
            WorkoutSessionId = 1,
            WorkoutSession = workoutSession,
            ExerciseId = 2,
            Exercise = exercise,
            OrderIndex = 3,
            Notes = "Focus on depth"
        };

        workoutExercise.WorkoutSessionId.ShouldBe(1);
        workoutExercise.WorkoutSession.ShouldBe(workoutSession);
        workoutExercise.ExerciseId.ShouldBe(2);
        workoutExercise.Exercise.ShouldBe(exercise);
        workoutExercise.OrderIndex.ShouldBe(3);
        workoutExercise.Notes.ShouldBe("Focus on depth");
    }

    [Test]
    public void NotesShouldDefaultToNull()
    {
        var workoutExercise = new WorkoutExercise();

        workoutExercise.Notes.ShouldBeNull();
    }
}
