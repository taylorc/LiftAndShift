using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Programmes.Commands.UpdateProgrammeSessionInputs;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class UpdateProgrammeSessionInputsCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private UpdateProgrammeSessionInputsCommandHandler _handler = null!;
    private const string UserId = "user-1";
    private const int SquatExerciseId = 1;

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new UpdateProgrammeSessionInputsCommandHandler(_context, _currentUser.Object);

        _context.Exercises.Add(new Exercise { Id = SquatExerciseId, Name = "Squat" });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    /// <summary>
    /// Session 1 logged with a successful Squat working set (prescription Squat@60); session 2
    /// pending with the prescription that success produced (Squat@65).
    /// </summary>
    private async Task<(UserProgramme Programme, ProgrammeSession First, ProgrammeSession Next)> SeedChainAsync()
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
        var first = new ProgrammeSession
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
        programme.Sessions.Add(first);
        programme.Sessions.Add(next);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);

        var workout = new WorkoutSession
        {
            UserId = UserId,
            Date = baseDate,
            Status = WorkoutStatus.Completed,
            IsProgrammeSession = true,
            ProgrammeSessionId = first.Id,
        };
        workout.Exercises.Add(new WorkoutExercise
        {
            ExerciseId = SquatExerciseId,
            OrderIndex = 0,
            Sets = { new WorkoutSet { SetNumber = 1, SetType = SetType.WorkingSet, WeightKg = 60m, Reps = 5, CompletedReps = 5, IsCompleted = true } },
        });
        _context.WorkoutSessions.Add(workout);
        await _context.SaveChangesAsync(CancellationToken.None);

        first.WorkoutSessionId = workout.Id;
        await _context.SaveChangesAsync(CancellationToken.None);

        return (programme, first, next);
    }

    [Test]
    public async Task ShouldOverrideTheSessionsPrescribedWeights()
    {
        var (programme, first, _) = await SeedChainAsync();

        await _handler.Handle(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = first.Id,
            LiftProgression = new() { ["Squat"] = 100m },
        }, CancellationToken.None);

        (await _context.ProgrammeSessions.FindAsync(first.Id))!.LiftProgression["Squat"].ShouldBe(100m);
    }

    [Test]
    public async Task ShouldReplayDownstreamFromTheOverriddenBaseline()
    {
        var (programme, first, next) = await SeedChainAsync();

        await _handler.Handle(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = first.Id,
            LiftProgression = new() { ["Squat"] = 100m },
        }, CancellationToken.None);

        // Session 1's Squat succeeded, so from a 100kg baseline the next session is 100 + 5 (heavy lift).
        (await _context.ProgrammeSessions.FindAsync(next.Id))!.LiftProgression["Squat"].ShouldBe(105m);
    }

    [Test]
    public async Task ShouldPreserveOtherLifts_WhenOverridingOneLiftsProgression()
    {
        var (programme, first, _) = await SeedChainAsync();
        first.LiftProgression["Bench"] = 40m;
        await _context.SaveChangesAsync(CancellationToken.None);

        await _handler.Handle(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = first.Id,
            LiftProgression = new() { ["Squat"] = 100m },
        }, CancellationToken.None);

        var updated = (await _context.ProgrammeSessions.FindAsync(first.Id))!;
        updated.LiftProgression["Squat"].ShouldBe(100m);
        updated.LiftProgression["Bench"].ShouldBe(40m);
    }

    [Test]
    public async Task ShouldOverrideConsecutiveFailures_WhenProvided()
    {
        var (programme, first, _) = await SeedChainAsync();

        await _handler.Handle(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = first.Id,
            ConsecutiveFailures = new() { ["Squat"] = 2 },
        }, CancellationToken.None);

        (await _context.ProgrammeSessions.FindAsync(first.Id))!.ConsecutiveFailures["Squat"].ShouldBe(2);
    }

    [Test]
    public async Task ShouldThrowNotFound_WhenProgrammeBelongsToAnotherUser()
    {
        var (programme, first, _) = await SeedChainAsync();
        _currentUser.Setup(u => u.Id).Returns("someone-else");

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = first.Id,
            LiftProgression = new() { ["Squat"] = 100m },
        }, CancellationToken.None).AsTask());
    }

    [Test]
    public async Task ShouldThrowNotFound_WhenSessionIsPendingNotLogged()
    {
        var (programme, _, next) = await SeedChainAsync();

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(new UpdateProgrammeSessionInputsCommand
        {
            UserProgrammeId = programme.Id,
            ProgrammeSessionId = next.Id,
            LiftProgression = new() { ["Squat"] = 100m },
        }, CancellationToken.None).AsTask());
    }
}
