using System.Linq;
using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Programmes.Commands.LogProgrammeSession;
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

public class LogProgrammeSessionCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private LogProgrammeSessionCommandHandler _handler = null!;
    private const string UserId = "user-1";
    private const int SquatExerciseId = 1;
    private const int BenchPressExerciseId = 2;

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new LogProgrammeSessionCommandHandler(_context, _currentUser.Object);

        _context.Exercises.Add(new Exercise { Id = SquatExerciseId, Name = "Squat" });
        _context.Exercises.Add(new Exercise { Id = BenchPressExerciseId, Name = "Bench Press" });
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private async Task<(UserProgramme Programme, ProgrammeSession Session)> SeedProgrammeAsync(
        string userId = UserId,
        WorkoutType workoutType = WorkoutType.A,
        Dictionary<string, int>? consecutiveFailures = null)
    {
        var programme = new UserProgramme
        {
            UserId = userId,
            ProgrammeTemplateId = "starting-strength",
            Status = ProgrammeStatus.Active,
            SessionCount = 0,
            CurrentWorkoutType = workoutType
        };
        var session = new ProgrammeSession
        {
            WorkoutType = workoutType,
            ScheduledDate = DateTimeOffset.UtcNow,
            LiftProgression = new() { ["Squat"] = 60m, ["Bench Press"] = 40m },
            ConsecutiveFailures = consecutiveFailures ?? new()
        };
        programme.Sessions.Add(session);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);
        return (programme, session);
    }

    private static LogWorkoutExerciseDto SuccessfulSquat() => new()
    {
        ExerciseId = SquatExerciseId,
        OrderIndex = 0,
        Sets = new()
        {
            new LogWorkoutSetDto { SetNumber = 1, WeightKg = 60m, Reps = 5, CompletedReps = 5, IsCompleted = true }
        }
    };

    private static LogWorkoutExerciseDto FailedSquat() => new()
    {
        ExerciseId = SquatExerciseId,
        OrderIndex = 0,
        Sets = new()
        {
            new LogWorkoutSetDto { SetNumber = 1, WeightKg = 60m, Reps = 5, CompletedReps = 3, IsCompleted = true }
        }
    };

    [Test]
    public async Task ShouldThrowNotFoundException_WhenProgrammeDoesNotExist()
    {
        var command = new LogProgrammeSessionCommand { UserProgrammeId = 999, ProgrammeSessionId = 1 };

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task ShouldThrowNotFoundException_WhenProgrammeBelongsToAnotherUser()
    {
        var (programme, session) = await SeedProgrammeAsync(userId: "other-user");

        var command = new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id
        };

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task ShouldThrowNotFoundException_WhenProgrammeSessionDoesNotBelongToProgramme()
    {
        var (programme, _) = await SeedProgrammeAsync();

        var command = new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = 999
        };

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task ShouldCreateCompletedWorkoutSessionLinkedToProgramme()
    {
        var (programme, session) = await SeedProgrammeAsync();

        var command = new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id,
            Exercises = new() { SuccessfulSquat() }
        };

        var workoutSessionId = await _handler.Handle(command, CancellationToken.None);

        var workoutSession = await _context.WorkoutSessions
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Sets)
            .FirstAsync(w => w.Id == workoutSessionId);

        workoutSession.UserId.ShouldBe(UserId);
        workoutSession.Status.ShouldBe(WorkoutStatus.Completed);
        workoutSession.IsProgrammeSession.ShouldBeTrue();
        workoutSession.ProgrammeSessionId.ShouldBe(session.Id);
        workoutSession.Exercises.Single().Sets.Single().WeightKg.ShouldBe(60m);
    }

    [Test]
    public async Task ShouldMarkProgrammeSessionCompletedAndLinkWorkoutSession()
    {
        var (programme, session) = await SeedProgrammeAsync();

        var workoutSessionId = await _handler.Handle(new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id
        }, CancellationToken.None);

        var updatedSession = await _context.ProgrammeSessions.FindAsync(session.Id);
        updatedSession!.CompletedDate.ShouldNotBeNull();
        updatedSession.WorkoutSessionId.ShouldBe(workoutSessionId);
    }

    [Test]
    public async Task ShouldIncrementSessionCountAndToggleWorkoutType()
    {
        var (programme, session) = await SeedProgrammeAsync(workoutType: WorkoutType.A);

        await _handler.Handle(new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id
        }, CancellationToken.None);

        var updatedProgramme = await _context.UserProgrammes.FindAsync(programme.Id);
        updatedProgramme!.SessionCount.ShouldBe(1);
        updatedProgramme.CurrentWorkoutType.ShouldBe(WorkoutType.B);
    }

    [Test]
    public async Task ShouldIncrementWeightAndResetFailures_WhenLiftSucceeds()
    {
        var (programme, session) = await SeedProgrammeAsync(
            consecutiveFailures: new() { ["Squat"] = 2 });

        await _handler.Handle(new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id,
            Exercises = new() { SuccessfulSquat() }
        }, CancellationToken.None);

        var nextSession = await _context.ProgrammeSessions
            .Where(s => s.UserProgrammeId == programme.Id && s.Id != session.Id)
            .SingleAsync(CancellationToken.None);

        // Squat is a heavy lift: +5kg
        nextSession.LiftProgression["Squat"].ShouldBe(65m);
        nextSession.ConsecutiveFailures["Squat"].ShouldBe(0);
    }

    [Test]
    public async Task ShouldHoldWeightAndIncrementFailures_WhenLiftFailsOnce()
    {
        var (programme, session) = await SeedProgrammeAsync();

        await _handler.Handle(new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id,
            Exercises = new() { FailedSquat() }
        }, CancellationToken.None);

        var nextSession = await _context.ProgrammeSessions
            .Where(s => s.UserProgrammeId == programme.Id && s.Id != session.Id)
            .SingleAsync(CancellationToken.None);

        // 1st consecutive failure: weight held, failure count now 1
        nextSession.LiftProgression["Squat"].ShouldBe(60m);
        nextSession.ConsecutiveFailures["Squat"].ShouldBe(1);
    }

    [Test]
    public async Task ShouldDeloadAndResetFailures_WhenThirdConsecutiveFailure()
    {
        var (programme, session) = await SeedProgrammeAsync(
            consecutiveFailures: new() { ["Squat"] = 2 });

        await _handler.Handle(new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id,
            Exercises = new() { FailedSquat() }
        }, CancellationToken.None);

        var nextSession = await _context.ProgrammeSessions
            .Where(s => s.UserProgrammeId == programme.Id && s.Id != session.Id)
            .SingleAsync(CancellationToken.None);

        // 3rd consecutive failure: 60 * 0.9 = 54, rounded to nearest 1.25 = 53.75, and the streak resets
        nextSession.LiftProgression["Squat"].ShouldBe(53.75m);
        nextSession.ConsecutiveFailures["Squat"].ShouldBe(0);
    }

    [Test]
    public async Task ShouldCarryLiftForwardUnchanged_WhenNotTrainedThisSession()
    {
        var (programme, session) = await SeedProgrammeAsync(
            consecutiveFailures: new() { ["Bench Press"] = 1 });

        // Only Squat is logged this session; Bench Press isn't trained
        await _handler.Handle(new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id,
            Exercises = new() { SuccessfulSquat() }
        }, CancellationToken.None);

        var nextSession = await _context.ProgrammeSessions
            .Where(s => s.UserProgrammeId == programme.Id && s.Id != session.Id)
            .SingleAsync(CancellationToken.None);

        nextSession.LiftProgression["Bench Press"].ShouldBe(40m);
        nextSession.ConsecutiveFailures["Bench Press"].ShouldBe(1);
    }

    [Test]
    public async Task ShouldCarryLiftForwardUnchanged_WhenNoWorkingSetsHaveCompletedReps()
    {
        var (programme, session) = await SeedProgrammeAsync(
            consecutiveFailures: new() { ["Squat"] = 1 });

        var uncheckedSquat = new LogWorkoutExerciseDto
        {
            ExerciseId = SquatExerciseId,
            OrderIndex = 0,
            Sets = new()
            {
                new LogWorkoutSetDto { SetNumber = 1, WeightKg = 60m, Reps = 5, CompletedReps = null, IsCompleted = false }
            }
        };

        await _handler.Handle(new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id,
            Exercises = new() { uncheckedSquat }
        }, CancellationToken.None);

        var nextSession = await _context.ProgrammeSessions
            .Where(s => s.UserProgrammeId == programme.Id && s.Id != session.Id)
            .SingleAsync(CancellationToken.None);

        nextSession.LiftProgression["Squat"].ShouldBe(60m);
        nextSession.ConsecutiveFailures["Squat"].ShouldBe(1);
    }

    [Test]
    public async Task ShouldIgnoreWarmupSets_WhenDeterminingOutcome()
    {
        var (programme, session) = await SeedProgrammeAsync();

        var squatWithFailedWarmup = new LogWorkoutExerciseDto
        {
            ExerciseId = SquatExerciseId,
            OrderIndex = 0,
            Sets = new()
            {
                new LogWorkoutSetDto { SetNumber = 1, SetType = SetType.Warmup, WeightKg = 20m, Reps = 5, CompletedReps = 2, IsCompleted = true },
                new LogWorkoutSetDto { SetNumber = 2, SetType = SetType.WorkingSet, WeightKg = 60m, Reps = 5, CompletedReps = 5, IsCompleted = true }
            }
        };

        await _handler.Handle(new LogProgrammeSessionCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = session.Id,
            Exercises = new() { squatWithFailedWarmup }
        }, CancellationToken.None);

        var nextSession = await _context.ProgrammeSessions
            .Where(s => s.UserProgrammeId == programme.Id && s.Id != session.Id)
            .SingleAsync(CancellationToken.None);

        // The warmup shortfall is irrelevant; the working set succeeded, so Squat progresses
        nextSession.LiftProgression["Squat"].ShouldBe(65m);
    }
}
