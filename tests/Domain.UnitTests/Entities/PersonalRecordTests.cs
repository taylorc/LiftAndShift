using System;
using LiftAndShift.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class PersonalRecordTests
{
    [Test]
    public void UserIdShouldDefaultToEmptyString()
    {
        var record = new PersonalRecord();

        record.UserId.ShouldBe(string.Empty);
    }

    [Test]
    public void ShouldRetainAssignedPropertyValues()
    {
        var exercise = new Exercise { Name = "Bench Press" };
        var achievedAt = DateTimeOffset.UtcNow;

        var record = new PersonalRecord
        {
            UserId = "user-1",
            ExerciseId = 5,
            Exercise = exercise,
            WeightKg = 100m,
            Reps = 5,
            AchievedAt = achievedAt,
            Estimated1RmKg = 116.7m
        };

        record.UserId.ShouldBe("user-1");
        record.ExerciseId.ShouldBe(5);
        record.Exercise.ShouldBe(exercise);
        record.WeightKg.ShouldBe(100m);
        record.Reps.ShouldBe(5);
        record.AchievedAt.ShouldBe(achievedAt);
        record.Estimated1RmKg.ShouldBe(116.7m);
    }
}
