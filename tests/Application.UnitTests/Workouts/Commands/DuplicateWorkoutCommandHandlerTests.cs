using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Application.Workouts.Commands.DuplicateWorkout;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Workouts.Commands;

public class DuplicateWorkoutCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private DuplicateWorkoutCommandHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new DuplicateWorkoutCommandHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private async Task<WorkoutSession> SeedSessionAsync()
    {
        var workoutExercise = new WorkoutExercise
        {
            ExerciseId = 4,
            OrderIndex = 0,
            Notes = "Exercise notes"
        };
        workoutExercise.Sets.Add(new WorkoutSet
        {
            SetNumber = 1,
            SetType = SetType.WorkingSet,
            WeightKg = 80m,
            Reps = 5,
            CompletedReps = 5,
            IsCompleted = true
        });

        var session = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow.AddDays(-3),
            Notes = "Original notes",
            Status = WorkoutStatus.Completed
        };
        session.Exercises.Add(workoutExercise);

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(CancellationToken.None);
        return session;
    }

    [Test]
    public async Task ShouldThrowNotFoundException_WhenSourceDoesNotExist()
    {
        await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(new DuplicateWorkoutCommand(999), CancellationToken.None).AsTask());
    }

    [Test]
    public async Task ShouldThrowNotFoundException_WhenSourceBelongsToAnotherUser()
    {
        var source = await SeedSessionAsync();
        source.UserId = "other-user";
        await _context.SaveChangesAsync(CancellationToken.None);

        await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(new DuplicateWorkoutCommand(source.Id), CancellationToken.None).AsTask());
    }

    [Test]
    public async Task ShouldCreateNewDraftSessionWithCopiedNotesAndExercises()
    {
        var source = await SeedSessionAsync();

        var newId = await _handler.Handle(new DuplicateWorkoutCommand(source.Id), CancellationToken.None);

        newId.ShouldNotBe(source.Id);

        var newSession = await _context.WorkoutSessions
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Sets)
            .FirstAsync(s => s.Id == newId);

        newSession.UserId.ShouldBe(UserId);
        newSession.Notes.ShouldBe("Original notes");
        newSession.Status.ShouldBe(WorkoutStatus.Draft);
        newSession.Exercises.Count.ShouldBe(1);

        var exercise = newSession.Exercises.Single();
        exercise.ExerciseId.ShouldBe(4);
        exercise.Notes.ShouldBe("Exercise notes");
        exercise.Sets.Count.ShouldBe(1);

        var set = exercise.Sets.Single();
        set.WeightKg.ShouldBe(80m);
        set.Reps.ShouldBe(5);
    }

    [Test]
    public async Task ShouldResetSetsToNotCompletedAndClearCompletedReps()
    {
        var source = await SeedSessionAsync();

        var newId = await _handler.Handle(new DuplicateWorkoutCommand(source.Id), CancellationToken.None);

        var newSet = await _context.WorkoutSets
            .Include(s => s.WorkoutExercise)
            .FirstAsync(s => s.WorkoutExercise.WorkoutSessionId == newId);

        newSet.IsCompleted.ShouldBeFalse();
        newSet.CompletedReps.ShouldBeNull();
    }
}
