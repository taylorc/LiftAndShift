using System;
using LiftAndShift.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class BodyMetricTests
{
    [Test]
    public void UserIdShouldDefaultToEmptyString()
    {
        var metric = new BodyMetric();

        metric.UserId.ShouldBe(string.Empty);
    }

    [Test]
    public void NotesShouldDefaultToNull()
    {
        var metric = new BodyMetric();

        metric.Notes.ShouldBeNull();
    }

    [Test]
    public void ShouldRetainAssignedPropertyValues()
    {
        var date = DateTimeOffset.UtcNow;
        var metric = new BodyMetric
        {
            UserId = "user-1",
            Date = date,
            WeightKg = 82.5m,
            Notes = "Feeling strong"
        };

        metric.UserId.ShouldBe("user-1");
        metric.Date.ShouldBe(date);
        metric.WeightKg.ShouldBe(82.5m);
        metric.Notes.ShouldBe("Feeling strong");
    }
}
