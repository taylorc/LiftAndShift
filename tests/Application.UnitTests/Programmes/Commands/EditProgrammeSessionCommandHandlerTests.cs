using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Programmes.Commands.EditProgrammeSession;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class EditProgrammeSessionCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private EditProgrammeSessionCommandHandler _handler = null!;
    private const string UserId = "user-1";
    private const int SquatExerciseId = 1;

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new EditProgrammeSessionCommandHandler(_context, _currentUser.Object);

        _context.Exercises.Add(new Exercise { Id = SquatExerciseId, Name = "Squat" });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    /// <summary>
    /// Seeds a two-session chain: session 1 logged with a successful Squat working set
    /// (its own prescription is Squat@60), session 2 pending with the prescription that
    /// a success produced (Squat@65, 0 failures).
    /// </summary>
    private async Task<(UserProgramme Programme, ProgrammeSession Logged, ProgrammeSession Next)> SeedChainAsync()
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var programme = new UserProgramme
        {
            UserId = UserId,
            ProgrammeTemplateId = "starting-strength",
            Status = ProgrammeStatus.Active,
            SessionCount = 1,
            CurrentWorkoutType = WorkoutType.B,
        };

        var logged = new ProgrammeSession
        {
            WorkoutType = WorkoutType.A,
            ScheduledDate = baseDate,
            CompletedDate = baseDate,
            LiftProgression = new() { ["Squat"] = 60m },
            ConsecutiveFailures = new(),
        };
        var next = new ProgrammeSession
        {
            WorkoutType = WorkoutType.B,
            ScheduledDate = baseDate.AddDays(2),
            LiftProgression = new() { ["Squat"] = 65m },
            ConsecutiveFailures = new() { ["Squat"] = 0 },
        };
        programme.Sessions.Add(logged);
        programme.Sessions.Add(next);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        var workout = new WorkoutSession
        {
            UserId = UserId,
            Date = baseDate,
            Status = WorkoutStatus.Completed,
            IsProgrammeSession = true,
            ProgrammeSessionId = logged.Id,
        };
        workout.Exercises.Add(new WorkoutExercise
        {
            ExerciseId = SquatExerciseId,
            OrderIndex = 0,
            Sets =
            {
                new WorkoutSet { SetNumber = 1, SetType = SetType.WorkingSet, WeightKg = 60m, Reps = 5, CompletedReps = 5, IsCompleted = true },
            },
        });
        _context.WorkoutSessions.Add(workout);
        await _context.SaveChangesAsync(CancellationToken.None);

        logged.WorkoutSessionId = workout.Id;
        await _context.SaveChangesAsync(CancellationToken.None);

        return (programme, logged, next);
    }

    private static EditProgrammeSessionCommand DowngradeSquatToFailure(int programmeId, int sessionId) => new()
    {
        UserProgrammeId = programmeId,
        ProgrammeSessionId = sessionId,
        Exercises = new()
        {
            new LogWorkoutExerciseDto
            {
                ExerciseId = SquatExerciseId,
                OrderIndex = 0,
                Sets = new()
                {
                    new LogWorkoutSetDto { SetNumber = 1, SetType = SetType.WorkingSet, WeightKg = 60m, Reps = 5, CompletedReps = 3, IsCompleted = true },
                },
            },
        },
    };

    [Test]
    public async Task ShouldReplayDownstreamSession_WhenLoggedOutcomeChanges()
    {
        var (programme, logged, next) = await SeedChainAsync();

        await _handler.Handle(DowngradeSquatToFailure(programme.Id, logged.Id), CancellationToken.None);

        var reloaded = await _context.ProgrammeSessions.FindAsync(next.Id);
        // Squat went from success to a first failure: weight held at 60, failure count now 1.
        reloaded!.LiftProgression["Squat"].ShouldBe(60m);
        reloaded.ConsecutiveFailures["Squat"].ShouldBe(1);
    }

    [Test]
    public async Task ShouldReplaceTheLoggedSetData()
    {
        var (programme, logged, _) = await SeedChainAsync();

        await _handler.Handle(DowngradeSquatToFailure(programme.Id, logged.Id), CancellationToken.None);

        var workout = await _context.WorkoutSessions
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .FirstAsync(w => w.ProgrammeSessionId == logged.Id);
        workout.Exercises.Single().Sets.Single().CompletedReps.ShouldBe(3);
    }

    [Test]
    public async Task ShouldLeaveTheEditedSessionsOwnPrescriptionUnchanged()
    {
        var (programme, logged, _) = await SeedChainAsync();

        await _handler.Handle(DowngradeSquatToFailure(programme.Id, logged.Id), CancellationToken.None);

        var reloaded = await _context.ProgrammeSessions.FindAsync(logged.Id);
        reloaded!.LiftProgression["Squat"].ShouldBe(60m);
    }

    [Test]
    public async Task ShouldThrowNotFound_WhenSessionIsPendingNotLogged()
    {
        var (programme, _, next) = await SeedChainAsync();

        await Should.ThrowAsync<NotFoundException>(
            () => _handler.Handle(DowngradeSquatToFailure(programme.Id, next.Id), CancellationToken.None).AsTask());
    }

    [Test]
    public async Task ShouldThrowNotFound_WhenProgrammeBelongsToAnotherUser()
    {
        var (programme, logged, _) = await SeedChainAsync();
        _currentUser.Setup(u => u.Id).Returns("someone-else");

        await Should.ThrowAsync<NotFoundException>(
            () => _handler.Handle(DowngradeSquatToFailure(programme.Id, logged.Id), CancellationToken.None).AsTask());
    }
}
