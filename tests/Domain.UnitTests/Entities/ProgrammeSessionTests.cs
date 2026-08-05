using System;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class ProgrammeSessionTests
{
    [Test]
    public void LiftProgressionShouldDefaultToEmptyDictionary()
    {
        var session = new ProgrammeSession();

        session.LiftProgression.ShouldBeEmpty();
    }

    [Test]
    public void WorkoutSessionIdAndCompletedDateShouldDefaultToNull()
    {
        var session = new ProgrammeSession();

        session.WorkoutSessionId.ShouldBeNull();
        session.CompletedDate.ShouldBeNull();
    }

    [Test]
    public void LiftProgressionShouldStoreAssignedWeights()
    {
        var session = new ProgrammeSession();

        session.LiftProgression["Squat"] = 100.0m;
        session.LiftProgression["Bench Press"] = 60.0m;

        session.LiftProgression["Squat"].ShouldBe(100.0m);
        session.LiftProgression["Bench Press"].ShouldBe(60.0m);
    }

    [Test]
    public void ShouldRetainAssignedPropertyValues()
    {
        var userProgramme = new UserProgramme { UserId = "user-1" };
        var scheduledDate = DateTimeOffset.UtcNow;
        var completedDate = scheduledDate.AddHours(1);

        var session = new ProgrammeSession
        {
            UserProgrammeId = 1,
            UserProgramme = userProgramme,
            WorkoutSessionId = 2,
            WorkoutType = WorkoutType.B,
            ScheduledDate = scheduledDate,
            CompletedDate = completedDate
        };

        session.UserProgrammeId.ShouldBe(1);
        session.UserProgramme.ShouldBe(userProgramme);
        session.WorkoutSessionId.ShouldBe(2);
        session.WorkoutType.ShouldBe(WorkoutType.B);
        session.ScheduledDate.ShouldBe(scheduledDate);
        session.CompletedDate.ShouldBe(completedDate);
    }
}
