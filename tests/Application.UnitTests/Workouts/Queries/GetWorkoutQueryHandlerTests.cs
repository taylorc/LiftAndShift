using System.Linq;
using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Application.Workouts.Queries.GetWorkout;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Workouts.Queries;

public class GetWorkoutQueryHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private GetWorkoutQueryHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new GetWorkoutQueryHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ShouldThrowNotFoundException_WhenSessionDoesNotExist()
    {
        await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(new GetWorkoutQuery(999), CancellationToken.None));
    }

    [Test]
    public async Task ShouldThrowNotFoundException_WhenSessionBelongsToAnotherUser()
    {
        var session = new WorkoutSession
        {
            UserId = "other-user",
            Date = DateTimeOffset.UtcNow
        };
        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(CancellationToken.None);

        await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(new GetWorkoutQuery(session.Id), CancellationToken.None));
    }

    [Test]
    public async Task ShouldReturnWorkoutDetailWithMappedExercisesAndSets()
    {
        var exercise1 = new Exercise { Name = "Squat" };
        var exercise2 = new Exercise { Name = "Bench Press" };
        _context.Exercises.AddRange(exercise1, exercise2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var workoutExercise2 = new WorkoutExercise { Exercise = exercise2, ExerciseId = exercise2.Id, OrderIndex = 1 };
        workoutExercise2.Sets.Add(new WorkoutSet { SetNumber = 2, WeightKg = 60m, Reps = 5 });
        workoutExercise2.Sets.Add(new WorkoutSet { SetNumber = 1, WeightKg = 50m, Reps = 5 });

        var workoutExercise1 = new WorkoutExercise { Exercise = exercise1, ExerciseId = exercise1.Id, OrderIndex = 0 };
        workoutExercise1.Sets.Add(new WorkoutSet { SetNumber = 1, WeightKg = 100m, Reps = 5 });

        var session = new WorkoutSession
        {
            UserId = UserId,
            Date = DateTimeOffset.UtcNow,
            Notes = "Good session",
            Status = WorkoutStatus.Completed,
            IsProgrammeSession = true
        };
        session.Exercises.Add(workoutExercise2);
        session.Exercises.Add(workoutExercise1);

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetWorkoutQuery(session.Id), CancellationToken.None);

        result.Id.ShouldBe(session.Id);
        result.Notes.ShouldBe("Good session");
        result.Status.ShouldBe("Completed");
        result.IsProgrammeSession.ShouldBeTrue();
        result.Exercises.Count.ShouldBe(2);

        var squat = result.Exercises.Single(e => e.ExerciseName == "Squat");
        squat.OrderIndex.ShouldBe(0);
        squat.Sets.Single().WeightKg.ShouldBe(100m);

        var bench = result.Exercises.Single(e => e.ExerciseName == "Bench Press");
        bench.OrderIndex.ShouldBe(1);
        bench.Sets.Select(s => s.SetNumber).OrderBy(n => n).ShouldBe(new[] { 1, 2 });
    }
}
