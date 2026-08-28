using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Exceptions;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Programmes.Commands.DeleteProgrammeSession;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class DeleteProgrammeSessionCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private DeleteProgrammeSessionCommandHandler _handler = null!;
    private const string UserId = "user-1";
    private const int SquatExerciseId = 1;

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new DeleteProgrammeSessionCommandHandler(_context, _currentUser.Object);

        _context.Exercises.Add(new Exercise { Id = SquatExerciseId, Name = "Squat" });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    /// <summary>
    /// Two logged sessions (A then B) plus the pending session (A) that logging B generated.
    /// </summary>
    private async Task<(UserProgramme Programme, ProgrammeSession FirstLogged, ProgrammeSession LatestLogged, ProgrammeSession Pending)> SeedAsync()
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var programme = new UserProgramme
        {
            UserId = UserId,
            ProgrammeTemplateId = "starting-strength",
            Status = ProgrammeStatus.Active,
            SessionCount = 2,
            CurrentWorkoutType = WorkoutType.A,
        };
        var firstLogged = new ProgrammeSession
        {
            WorkoutType = WorkoutType.A,
            ScheduledDate = baseDate,
            CompletedDate = baseDate,
            LiftProgression = new() { ["Squat"] = 60m },
            ConsecutiveFailures = new(),
        };
        var latestLogged = new ProgrammeSession
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
        programme.Sessions.Add(firstLogged);
        programme.Sessions.Add(latestLogged);
        programme.Sessions.Add(pending);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        await LinkWorkoutAsync(firstLogged, baseDate);
        await LinkWorkoutAsync(latestLogged, baseDate.AddDays(2));

        return (programme, firstLogged, latestLogged, pending);
    }

    private async Task LinkWorkoutAsync(ProgrammeSession session, DateTimeOffset date)
    {
        var workout = new WorkoutSession
        {
            UserId = UserId,
            Date = date,
            Status = WorkoutStatus.Completed,
            IsProgrammeSession = true,
            ProgrammeSessionId = session.Id,
        };
        workout.Exercises.Add(new WorkoutExercise
        {
            ExerciseId = SquatExerciseId,
            OrderIndex = 0,
            Sets = { new WorkoutSet { SetNumber = 1, SetType = SetType.WorkingSet, WeightKg = 60m, Reps = 5, CompletedReps = 5, IsCompleted = true } },
        });
        _context.WorkoutSessions.Add(workout);
        await _context.SaveChangesAsync(CancellationToken.None);

        session.WorkoutSessionId = workout.Id;
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    private static DeleteProgrammeSessionCommand Command(int programmeId, int sessionId) =>
        new() { UserProgrammeId = programmeId, ProgrammeSessionId = sessionId };

    [Test]
    public async Task ShouldRevertLatestLoggedSessionToPending()
    {
        var (programme, _, latest, _) = await SeedAsync();

        await _handler.Handle(Command(programme.Id, latest.Id), CancellationToken.None);

        var reloaded = await _context.ProgrammeSessions.FindAsync(latest.Id);
        reloaded!.CompletedDate.ShouldBeNull();
        reloaded.WorkoutSessionId.ShouldBeNull();
    }

    [Test]
    public async Task ShouldDeleteTheLinkedWorkoutSession()
    {
        var (programme, _, latest, _) = await SeedAsync();
        var workoutId = latest.WorkoutSessionId!.Value;

        await _handler.Handle(Command(programme.Id, latest.Id), CancellationToken.None);

        (await _context.WorkoutSessions.FindAsync(workoutId)).ShouldBeNull();
        // Only the first logged session's workout data remains.
        (await _context.WorkoutSets.CountAsync()).ShouldBe(1);
        (await _context.WorkoutExercises.CountAsync()).ShouldBe(1);
    }

    [Test]
    public async Task ShouldRemoveTheGeneratedNextPendingSession()
    {
        var (programme, _, latest, pending) = await SeedAsync();

        await _handler.Handle(Command(programme.Id, latest.Id), CancellationToken.None);

        (await _context.ProgrammeSessions.FindAsync(pending.Id)).ShouldBeNull();
    }

    [Test]
    public async Task ShouldDecrementSessionCountAndRestoreWorkoutType()
    {
        var (programme, _, latest, _) = await SeedAsync();

        await _handler.Handle(Command(programme.Id, latest.Id), CancellationToken.None);

        var reloaded = await _context.UserProgrammes.FindAsync(programme.Id);
        reloaded!.SessionCount.ShouldBe(1);
        reloaded.CurrentWorkoutType.ShouldBe(WorkoutType.B); // latest logged was a B session
    }

    [Test]
    public async Task ShouldThrowValidation_WhenTargetIsNotTheLatestLoggedSession()
    {
        var (programme, first, _, _) = await SeedAsync();

        await Should.ThrowAsync<ValidationException>(
            () => _handler.Handle(Command(programme.Id, first.Id), CancellationToken.None).AsTask());
    }

    [Test]
    public async Task ShouldThrowNotFound_WhenProgrammeBelongsToAnotherUser()
    {
        var (programme, _, latest, _) = await SeedAsync();
        _currentUser.Setup(u => u.Id).Returns("someone-else");

        await Should.ThrowAsync<NotFoundException>(
            () => _handler.Handle(Command(programme.Id, latest.Id), CancellationToken.None).AsTask());
    }
}
