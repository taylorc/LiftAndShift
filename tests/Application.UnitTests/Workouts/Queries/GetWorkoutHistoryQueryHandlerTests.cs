using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Application.Workouts.Queries.GetWorkoutHistory;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Workouts.Queries;

public class GetWorkoutHistoryQueryHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private GetWorkoutHistoryQueryHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new GetWorkoutHistoryQueryHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ShouldReturnEmptyList_WhenNoSessionsExist()
    {
        var result = await _handler.Handle(new GetWorkoutHistoryQuery(), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldOnlyReturnSessionsForCurrentUser()
    {
        _context.WorkoutSessions.Add(new WorkoutSession { UserId = "other-user", Date = DateTimeOffset.UtcNow });
        _context.WorkoutSessions.Add(new WorkoutSession { UserId = UserId, Date = DateTimeOffset.UtcNow });
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetWorkoutHistoryQuery(), CancellationToken.None);

        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task ShouldOrderSessionsByDateDescending()
    {
        var older = new WorkoutSession { UserId = UserId, Date = DateTimeOffset.UtcNow.AddDays(-2) };
        var newer = new WorkoutSession { UserId = UserId, Date = DateTimeOffset.UtcNow };
        _context.WorkoutSessions.AddRange(older, newer);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetWorkoutHistoryQuery(), CancellationToken.None);

        result[0].Id.ShouldBe(newer.Id);
        result[1].Id.ShouldBe(older.Id);
    }

    [Test]
    public async Task ShouldComputeTotalVolumeFromCompletedSetsOnly()
    {
        var exercise = new Exercise { Name = "Squat" };
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync(CancellationToken.None);

        var workoutExercise = new WorkoutExercise { Exercise = exercise, ExerciseId = exercise.Id };
        workoutExercise.Sets.Add(new WorkoutSet { SetNumber = 1, WeightKg = 100m, Reps = 5, CompletedReps = 5, IsCompleted = true });
        workoutExercise.Sets.Add(new WorkoutSet { SetNumber = 2, WeightKg = 100m, Reps = 5, IsCompleted = false });

        var session = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Completed
        };
        session.Exercises.Add(workoutExercise);

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetWorkoutHistoryQuery(), CancellationToken.None);

        var item = result.Single();
        item.TotalVolumeKg.ShouldBe(500m);
        item.ExerciseNames.ShouldBe(new List<string> { "Squat" });
        item.Status.ShouldBe("Completed");
    }
}
