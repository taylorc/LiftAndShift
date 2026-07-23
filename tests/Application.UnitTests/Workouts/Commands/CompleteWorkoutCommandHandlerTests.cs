using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Application.Workouts.Commands.CompleteWorkout;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Workouts.Commands;

public class CompleteWorkoutCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private CompleteWorkoutCommandHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new CompleteWorkoutCommandHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private async Task<WorkoutSession> SeedSessionAsync(WorkoutSet set, int exerciseId = 1)
    {
        var workoutExercise = new WorkoutExercise { ExerciseId = exerciseId };
        workoutExercise.Sets.Add(set);

        var session = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Draft
        };
        session.Exercises.Add(workoutExercise);

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(CancellationToken.None);
        return session;
    }

    [Test]
    public async Task ShouldThrowNotFoundException_WhenSessionDoesNotExist()
    {
        var command = new CompleteWorkoutCommand(999);

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task ShouldThrowNotFoundException_WhenSessionBelongsToAnotherUser()
    {
        var session = await SeedSessionAsync(new WorkoutSet { SetNumber = 1, WeightKg = 50m, Reps = 5 });
        session.UserId = "other-user";
        await _context.SaveChangesAsync(CancellationToken.None);

        await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(new CompleteWorkoutCommand(session.Id), CancellationToken.None));
    }

    [Test]
    public async Task ShouldMarkSessionAsCompleted()
    {
        var session = await SeedSessionAsync(new WorkoutSet
        {
            SetNumber = 1,
            SetType = SetType.WorkingSet,
            WeightKg = 100m,
            Reps = 5,
            CompletedReps = 5,
            IsCompleted = true
        });

        await _handler.Handle(new CompleteWorkoutCommand(session.Id), CancellationToken.None);

        var updated = await _context.WorkoutSessions.FindAsync(session.Id);
        updated!.Status.ShouldBe(WorkoutStatus.Completed);
    }

    [Test]
    public async Task ShouldCreatePersonalRecord_WhenCompletedWorkingSetBeatsNoExistingRecord()
    {
        var session = await SeedSessionAsync(new WorkoutSet
        {
            SetNumber = 1,
            SetType = SetType.WorkingSet,
            WeightKg = 100m,
            Reps = 5,
            CompletedReps = 5,
            IsCompleted = true
        }, exerciseId: 7);

        await _handler.Handle(new CompleteWorkoutCommand(session.Id), CancellationToken.None);

        var pr = await _context.PersonalRecords.FirstOrDefaultAsync(p => p.UserId == UserId && p.ExerciseId == 7);
        pr.ShouldNotBeNull();
        pr.WeightKg.ShouldBe(100m);
        pr.Reps.ShouldBe(5);
        pr.Estimated1RmKg.ShouldBe(Math.Round(100m * (1 + 5 / 30m), 2));
    }

    [Test]
    public async Task ShouldNotUpdatePersonalRecord_WhenExistingRecordIsHigher()
    {
        _context.PersonalRecords.Add(new PersonalRecord
        {
            UserId = UserId,
            ExerciseId = 7,
            WeightKg = 150m,
            Reps = 5,
            Estimated1RmKg = 175m
        });
        await _context.SaveChangesAsync(CancellationToken.None);

        var session = await SeedSessionAsync(new WorkoutSet
        {
            SetNumber = 1,
            SetType = SetType.WorkingSet,
            WeightKg = 100m,
            Reps = 5,
            CompletedReps = 5,
            IsCompleted = true
        }, exerciseId: 7);

        await _handler.Handle(new CompleteWorkoutCommand(session.Id), CancellationToken.None);

        var pr = await _context.PersonalRecords.SingleAsync(p => p.UserId == UserId && p.ExerciseId == 7);
        pr.WeightKg.ShouldBe(150m);
        pr.Estimated1RmKg.ShouldBe(175m);
    }

    [Test]
    public async Task ShouldIgnoreIncompleteOrWarmupOrZeroWeightSets()
    {
        var workoutExercise = new WorkoutExercise { ExerciseId = 3 };
        workoutExercise.Sets.Add(new WorkoutSet { SetNumber = 1, SetType = SetType.WorkingSet, WeightKg = 100m, Reps = 5, IsCompleted = false });
        workoutExercise.Sets.Add(new WorkoutSet { SetNumber = 2, SetType = SetType.Warmup, WeightKg = 100m, Reps = 5, IsCompleted = true });
        workoutExercise.Sets.Add(new WorkoutSet { SetNumber = 3, SetType = SetType.WorkingSet, WeightKg = 0m, Reps = 5, IsCompleted = true });

        var session = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Draft
        };
        session.Exercises.Add(workoutExercise);

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(CancellationToken.None);

        await _handler.Handle(new CompleteWorkoutCommand(session.Id), CancellationToken.None);

        var pr = await _context.PersonalRecords.FirstOrDefaultAsync(p => p.UserId == UserId && p.ExerciseId == 3);
        pr.ShouldBeNull();
    }
}
