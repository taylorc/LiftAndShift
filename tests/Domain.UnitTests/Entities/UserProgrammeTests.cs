using System;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class UserProgrammeTests
{
    [Test]
    public void StatusShouldDefaultToActive()
    {
        var programme = new UserProgramme();

        programme.Status.ShouldBe(ProgrammeStatus.Active);
    }

    [Test]
    public void CurrentWorkoutTypeShouldDefaultToA()
    {
        var programme = new UserProgramme();

        programme.CurrentWorkoutType.ShouldBe(WorkoutType.A);
    }

    [Test]
    public void SessionsShouldBeEmptyByDefault()
    {
        var programme = new UserProgramme();

        programme.Sessions.ShouldBeEmpty();
    }

    [Test]
    public void SessionsShouldContainAddedSession()
    {
        var programme = new UserProgramme();
        var session = new ProgrammeSession();

        programme.Sessions.Add(session);

        programme.Sessions.ShouldContain(session);
    }

    [Test]
    public void ShouldRetainAssignedPropertyValues()
    {
        var startedAt = DateTimeOffset.UtcNow;

        var programme = new UserProgramme
        {
            UserId = "user-1",
            ProgrammeTemplateId = "template-1",
            StartedAt = startedAt,
            Status = ProgrammeStatus.Paused,
            SessionCount = 3,
            CurrentWorkoutType = WorkoutType.B
        };

        programme.UserId.ShouldBe("user-1");
        programme.ProgrammeTemplateId.ShouldBe("template-1");
        programme.StartedAt.ShouldBe(startedAt);
        programme.Status.ShouldBe(ProgrammeStatus.Paused);
        programme.SessionCount.ShouldBe(3);
        programme.CurrentWorkoutType.ShouldBe(WorkoutType.B);
    }
}
