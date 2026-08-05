using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Workouts.Commands;

public class LogWorkoutCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private LogWorkoutCommandHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new LogWorkoutCommandHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ShouldCreateWorkoutSessionForCurrentUser()
    {
        var command = new LogWorkoutCommand
        {
            Date = DateTimeOffset.UtcNow,
            Notes = "Leg day",
            Complete = false,
            Exercises = new List<LogWorkoutExerciseDto>()
        };

        var id = await _handler.Handle(command, CancellationToken.None);

        var session = await _context.WorkoutSessions.FindAsync(id);
        session.ShouldNotBeNull();
        session.UserId.ShouldBe(UserId);
        session.Notes.ShouldBe("Leg day");
        session.Status.ShouldBe(WorkoutStatus.Draft);
    }

    [Test]
    public async Task ShouldMarkSessionCompletedWhenCompleteIsTrue()
    {
        var command = new LogWorkoutCommand { Complete = true };

        var id = await _handler.Handle(command, CancellationToken.None);

        var session = await _context.WorkoutSessions.FindAsync(id);
        session!.Status.ShouldBe(WorkoutStatus.Completed);
    }

    [Test]
    public async Task ShouldAddExercisesAndSets()
    {
        var command = new LogWorkoutCommand
        {
            Exercises = new List<LogWorkoutExerciseDto>
            {
                new()
                {
                    ExerciseId = 5,
                    OrderIndex = 0,
                    Notes = "Focus form",
                    Sets = new List<LogWorkoutSetDto>
                    {
                        new()
                        {
                            SetNumber = 1,
                            SetType = SetType.WorkingSet,
                            WeightKg = 100m,
                            Reps = 5,
                            CompletedReps = 5,
                            IsCompleted = true
                        }
                    }
                }
            }
        };

        var id = await _handler.Handle(command, CancellationToken.None);

        var session = await _context.WorkoutSessions
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Sets)
            .FirstAsync(s => s.Id == id);

        session.Exercises.Count.ShouldBe(1);
        var exercise = session.Exercises.Single();
        exercise.ExerciseId.ShouldBe(5);
        exercise.Notes.ShouldBe("Focus form");
        exercise.Sets.Count.ShouldBe(1);
        var set = exercise.Sets.Single();
        set.WeightKg.ShouldBe(100m);
        set.Reps.ShouldBe(5);
        set.CompletedReps.ShouldBe(5);
        set.IsCompleted.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnGeneratedSessionId()
    {
        var id1 = await _handler.Handle(new LogWorkoutCommand(), CancellationToken.None);
        var id2 = await _handler.Handle(new LogWorkoutCommand(), CancellationToken.None);

        id1.ShouldNotBe(0);
        id2.ShouldNotBe(id1);
    }
}
