using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Programmes.Queries.GetProgrammeSessions;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Queries;

public class GetProgrammeSessionsQueryHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private GetProgrammeSessionsQueryHandler _handler = null!;
    private const string UserId = "user-1";
    private const int SquatExerciseId = 1;

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new GetProgrammeSessionsQueryHandler(_context, _currentUser.Object);

        _context.Exercises.Add(new Exercise { Id = SquatExerciseId, Name = "Squat" });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private async Task<UserProgramme> SeedProgrammeAsync(string userId = UserId)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var programme = new UserProgramme
        {
            UserId = userId,
            ProgrammeTemplateId = "starting-strength",
            Status = ProgrammeStatus.Active,
            SessionCount = 2,
            CurrentWorkoutType = WorkoutType.A,
        };

        var loggedA = new ProgrammeSession
        {
            WorkoutType = WorkoutType.A,
            ScheduledDate = baseDate,
            CompletedDate = baseDate,
            LiftProgression = new() { ["Squat"] = 60m },
            ConsecutiveFailures = new(),
        };
        var loggedB = new ProgrammeSession
        {
            WorkoutType = WorkoutType.B,
            ScheduledDate = baseDate.AddDays(2),
            CompletedDate = baseDate.AddDays(2),
            LiftProgression = new() { ["Squat"] = 65m },
            ConsecutiveFailures = new(),
        };
        var pending = new ProgrammeSession
        {
            WorkoutType = WorkoutType.A,
            ScheduledDate = baseDate.AddDays(4),
            LiftProgression = new() { ["Squat"] = 70m },
            ConsecutiveFailures = new(),
        };
        programme.Sessions.Add(loggedA);
        programme.Sessions.Add(loggedB);
        programme.Sessions.Add(pending);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        await LinkWorkoutAsync(loggedA, completedReps: 5, date: baseDate);
        await LinkWorkoutAsync(loggedB, completedReps: 4, date: baseDate.AddDays(2));

        return programme;
    }

    private async Task LinkWorkoutAsync(ProgrammeSession session, int completedReps, DateTimeOffset date)
    {
        var workout = new WorkoutSession
        {
            UserId = session.UserProgramme?.UserId ?? UserId,
            Date = date,
            Status = WorkoutStatus.Completed,
            IsProgrammeSession = true,
            ProgrammeSessionId = session.Id,
        };
        workout.Exercises.Add(new WorkoutExercise
        {
            ExerciseId = SquatExerciseId,
            OrderIndex = 0,
            Sets =
            {
                new WorkoutSet { SetNumber = 1, SetType = SetType.WorkingSet, WeightKg = 60m, Reps = 5, CompletedReps = completedReps, IsCompleted = true },
            },
        });
        _context.WorkoutSessions.Add(workout);
        await _context.SaveChangesAsync(CancellationToken.None);

        session.WorkoutSessionId = workout.Id;
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    [Test]
    public async Task ShouldReturnOnlyLoggedSessions_WithTheirSetData()
    {
        var programme = await SeedProgrammeAsync();

        var result = await _handler.Handle(new GetProgrammeSessionsQuery(programme.Id), CancellationToken.None);

        result.Count.ShouldBe(2); // the pending session is excluded
        result[0].WorkoutType.ShouldBe("A");
        result[0].Exercises.Single().ExerciseName.ShouldBe("Squat");
        result[0].Exercises.Single().Sets.Single().CompletedReps.ShouldBe(5);
        result[1].Exercises.Single().Sets.Single().CompletedReps.ShouldBe(4);
    }

    [Test]
    public async Task ShouldOrderSessionsChronologically()
    {
        var programme = await SeedProgrammeAsync();

        var result = await _handler.Handle(new GetProgrammeSessionsQuery(programme.Id), CancellationToken.None);

        result.Select(s => s.WorkoutType).ShouldBe(new[] { "A", "B" });
    }

    [Test]
    public async Task ShouldReturnEmptyList_WhenNothingLoggedYet()
    {
        var programme = new UserProgramme
        {
            UserId = UserId,
            ProgrammeTemplateId = "starting-strength",
            Status = ProgrammeStatus.Active,
        };
        programme.Sessions.Add(new ProgrammeSession { WorkoutType = WorkoutType.A, ScheduledDate = DateTimeOffset.UtcNow });
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetProgrammeSessionsQuery(programme.Id), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldThrowNotFound_WhenProgrammeBelongsToAnotherUser()
    {
        var programme = await SeedProgrammeAsync(userId: "someone-else");

        await Should.ThrowAsync<NotFoundException>(
            () => _handler.Handle(new GetProgrammeSessionsQuery(programme.Id), CancellationToken.None).AsTask());
    }
}
