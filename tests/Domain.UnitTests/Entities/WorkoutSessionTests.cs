using System;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class WorkoutSessionTests
{
    [Test]
    public void StatusShouldDefaultToDraft()
    {
        var session = new WorkoutSession();

        session.Status.ShouldBe(WorkoutStatus.Draft);
    }

    [Test]
    public void IsProgrammeSessionShouldDefaultToFalse()
    {
        var session = new WorkoutSession();

        session.IsProgrammeSession.ShouldBeFalse();
    }

    [Test]
    public void ExercisesShouldBeEmptyByDefault()
    {
        var session = new WorkoutSession();

        session.Exercises.ShouldBeEmpty();
    }

    [Test]
    public void ExercisesShouldContainAddedExercise()
    {
        var session = new WorkoutSession();
        var exercise = new WorkoutExercise();

        session.Exercises.Add(exercise);

        session.Exercises.ShouldContain(exercise);
    }

    [Test]
    public void ShouldRetainAssignedPropertyValues()
    {
        var date = DateTimeOffset.UtcNow;

        var session = new WorkoutSession
        {
            UserId = "user-1",
            Date = date,
            Notes = "Good session",
            Status = WorkoutStatus.Completed,
            IsProgrammeSession = true,
            ProgrammeSessionId = 4
        };

        session.UserId.ShouldBe("user-1");
        session.Date.ShouldBe(date);
        session.Notes.ShouldBe("Good session");
        session.Status.ShouldBe(WorkoutStatus.Completed);
        session.IsProgrammeSession.ShouldBeTrue();
        session.ProgrammeSessionId.ShouldBe(4);
    }
}
