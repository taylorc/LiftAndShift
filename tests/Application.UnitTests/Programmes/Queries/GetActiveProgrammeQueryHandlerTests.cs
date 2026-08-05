using System.Linq;
using LiftAndShift.Application.Calculators;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Programmes.Queries.GetActiveProgramme;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Queries;

public class GetActiveProgrammeQueryHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private GetActiveProgrammeQueryHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new GetActiveProgrammeQueryHandler(_context, _currentUser.Object, new WarmupCalculatorService());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ShouldReturnNull_WhenNoActiveProgrammeExists()
    {
        var result = await _handler.Handle(new GetActiveProgrammeQuery(), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Test]
    public async Task ShouldIgnoreProgrammesBelongingToOtherUsersOrNotActive()
    {
        _context.UserProgrammes.Add(new UserProgramme { UserId = "other-user", Status = ProgrammeStatus.Active });
        _context.UserProgrammes.Add(new UserProgramme { UserId = UserId, Status = ProgrammeStatus.Abandoned });
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetActiveProgrammeQuery(), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Test]
    public async Task ShouldMapActiveProgrammeProperties()
    {
        var programme = new UserProgramme
        {
            UserId = UserId,
            ProgrammeTemplateId = "starting-strength",
            StartedAt = DateTimeOffset.UtcNow.AddDays(-10),
            Status = ProgrammeStatus.Active,
            SessionCount = 4
        };
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetActiveProgrammeQuery(), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(programme.Id);
        result.ProgrammeTemplateId.ShouldBe("starting-strength");
        result.ProgrammeName.ShouldBe("Starting Strength");
        result.Status.ShouldBe("Active");
        result.SessionCount.ShouldBe(4);
        result.NextSession.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnEarliestUncompletedSessionAsNextSession()
    {
        var programme = new UserProgramme { UserId = UserId, Status = ProgrammeStatus.Active };
        var completed = new ProgrammeSession
        {
            WorkoutType = WorkoutType.A,
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(-5),
            CompletedDate = DateTimeOffset.UtcNow.AddDays(-4)
        };
        var laterUncompleted = new ProgrammeSession
        {
            WorkoutType = WorkoutType.B,
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(3)
        };
        var earlierUncompleted = new ProgrammeSession
        {
            WorkoutType = WorkoutType.A,
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(1),
            LiftProgression = new() { ["Squat"] = 80m }
        };
        programme.Sessions.Add(completed);
        programme.Sessions.Add(laterUncompleted);
        programme.Sessions.Add(earlierUncompleted);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetActiveProgrammeQuery(), CancellationToken.None);

        result!.NextSession.ShouldNotBeNull();
        result.NextSession!.SessionId.ShouldBe(earlierUncompleted.Id);
        result.NextSession.WorkoutType.ShouldBe("A");
    }

    [Test]
    public async Task ShouldPrescribeWorkoutALiftsWithProgressionWeightAndDefaultFallback()
    {
        var programme = new UserProgramme { UserId = UserId, Status = ProgrammeStatus.Active };
        var nextSession = new ProgrammeSession
        {
            WorkoutType = WorkoutType.A,
            ScheduledDate = DateTimeOffset.UtcNow,
            LiftProgression = new() { ["Squat"] = 80m }
        };
        programme.Sessions.Add(nextSession);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetActiveProgrammeQuery(), CancellationToken.None);

        var lifts = result!.NextSession!.PrescribedLifts;
        lifts.Select(l => l.LiftName).ShouldBe(new[] { "Squat", "Bench Press", "Deadlift" });

        var squat = lifts.Single(l => l.LiftName == "Squat");
        squat.WeightKg.ShouldBe(80m);
        squat.Sets.ShouldBe(3);
        squat.Reps.ShouldBe(5);
        squat.WarmupSets.ShouldNotBeEmpty();

        var benchPress = lifts.Single(l => l.LiftName == "Bench Press");
        benchPress.WeightKg.ShouldBe(20m);

        var deadlift = lifts.Single(l => l.LiftName == "Deadlift");
        deadlift.Sets.ShouldBe(1);
    }

    [Test]
    public async Task ShouldPrescribeWorkoutBLifts_WhenNextSessionIsWorkoutB()
    {
        var programme = new UserProgramme { UserId = UserId, Status = ProgrammeStatus.Active };
        var nextSession = new ProgrammeSession
        {
            WorkoutType = WorkoutType.B,
            ScheduledDate = DateTimeOffset.UtcNow
        };
        programme.Sessions.Add(nextSession);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetActiveProgrammeQuery(), CancellationToken.None);

        result!.NextSession!.PrescribedLifts.Select(l => l.LiftName)
            .ShouldBe(new[] { "Squat", "Overhead Press", "Deadlift" });
    }
}
