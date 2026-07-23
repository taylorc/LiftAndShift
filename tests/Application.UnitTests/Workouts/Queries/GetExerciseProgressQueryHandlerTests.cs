using System.Linq;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Application.Workouts.Queries.GetExerciseProgress;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Workouts.Queries;

public class GetExerciseProgressQueryHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private GetExerciseProgressQueryHandler _handler = null!;
    private const string UserId = "user-1";
    private const int ExerciseId = 10;

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new GetExerciseProgressQueryHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private static WorkoutExercise CreateExerciseWithSets(int exerciseId, params WorkoutSet[] sets)
    {
        var exercise = new WorkoutExercise { ExerciseId = exerciseId };
        foreach (var set in sets)
        {
            exercise.Sets.Add(set);
        }
        return exercise;
    }

    [Test]
    public async Task ShouldReturnEmptyList_WhenNoCompletedSessionsExist()
    {
        var result = await _handler.Handle(new GetExerciseProgressQuery(ExerciseId), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldIgnoreDraftSessions()
    {
        var session = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Draft
        };
        session.Exercises.Add(CreateExerciseWithSets(ExerciseId,
            new WorkoutSet { SetType = SetType.WorkingSet, WeightKg = 100m, Reps = 5, IsCompleted = true }));

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExerciseProgressQuery(ExerciseId), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldIgnoreOtherExercisesAndOtherUsers()
    {
        var mine = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Completed
        };
        mine.Exercises.Add(CreateExerciseWithSets(999,
            new WorkoutSet { SetType = SetType.WorkingSet, WeightKg = 50m, Reps = 5, IsCompleted = true }));

        var others = new WorkoutSession
        {
            UserId = "other-user",
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Completed
        };
        others.Exercises.Add(CreateExerciseWithSets(ExerciseId,
            new WorkoutSet { SetType = SetType.WorkingSet, WeightKg = 200m, Reps = 5, IsCompleted = true }));

        _context.WorkoutSessions.AddRange(mine, others);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExerciseProgressQuery(ExerciseId), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldComputeMaxWeightEstimated1RmAndTotalVolume()
    {
        var date = DateTimeOffset.UtcNow;
        var session = new WorkoutSession
        {
            UserId = UserId,
            Date = date,
            Status = WorkoutStatus.Completed
        };
        session.Exercises.Add(CreateExerciseWithSets(ExerciseId,
            new WorkoutSet { SetType = SetType.WorkingSet, WeightKg = 80m, Reps = 5, CompletedReps = 5, IsCompleted = true },
            new WorkoutSet { SetType = SetType.WorkingSet, WeightKg = 100m, Reps = 5, CompletedReps = 5, IsCompleted = true },
            new WorkoutSet { SetType = SetType.Warmup, WeightKg = 200m, Reps = 5, IsCompleted = true },
            new WorkoutSet { SetType = SetType.WorkingSet, WeightKg = 100m, Reps = 5, IsCompleted = false }));

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExerciseProgressQuery(ExerciseId), CancellationToken.None);

        var point = result.Single();
        point.Date.ShouldBe(date);
        point.MaxWeightKg.ShouldBe(100m);
        point.Estimated1Rm.ShouldBe(Math.Round(100m * (1 + 5 / 30m), 2));
        point.TotalVolumeKg.ShouldBe(80m * 5 + 100m * 5);
    }

    [Test]
    public async Task ShouldOrderPointsByDateAscending()
    {
        var earlier = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow.AddDays(-5),
            Status = WorkoutStatus.Completed
        };
        earlier.Exercises.Add(CreateExerciseWithSets(ExerciseId,
            new WorkoutSet { SetType = SetType.WorkingSet, WeightKg = 60m, Reps = 5, IsCompleted = true }));

        var later = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Completed
        };
        later.Exercises.Add(CreateExerciseWithSets(ExerciseId,
            new WorkoutSet { SetType = SetType.WorkingSet, WeightKg = 70m, Reps = 5, IsCompleted = true }));

        _context.WorkoutSessions.AddRange(later, earlier);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExerciseProgressQuery(ExerciseId), CancellationToken.None);

        result[0].MaxWeightKg.ShouldBe(60m);
        result[1].MaxWeightKg.ShouldBe(70m);
    }
}
