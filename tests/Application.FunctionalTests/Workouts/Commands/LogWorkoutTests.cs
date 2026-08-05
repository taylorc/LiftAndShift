using LiftAndShift.Application.Common.Exceptions;
using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.FunctionalTests.Workouts.Commands;

public class LogWorkoutTests : TestBase
{
    [Test]
    public async Task ShouldRequireAuthenticatedUser()
    {
        var command = new LogWorkoutCommand
        {
            Date = DateTimeOffset.UtcNow,
            Complete = false,
            Exercises = new List<LogWorkoutExerciseDto>()
        };

        await Should.ThrowAsync<UnauthorizedAccessException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldCreateWorkoutSessionWithExercisesAndSets()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        // Seed an exercise first
        var exercise = new Exercise
        {
            Name = "Test Squat",
            MuscleGroup = MuscleGroup.Legs,
            EquipmentType = EquipmentType.Barbell,
            MovementPattern = MovementPattern.Squat,
            IsCustom = false,
            IsActive = true
        };
        await TestApp.AddAsync(exercise);

        var command = new LogWorkoutCommand
        {
            Date = DateTimeOffset.UtcNow,
            Notes = "Test workout",
            Complete = false,
            Exercises = new List<LogWorkoutExerciseDto>
            {
                new()
                {
                    ExerciseId = exercise.Id,
                    OrderIndex = 0,
                    Sets = new List<LogWorkoutSetDto>
                    {
                        new() { SetNumber = 1, SetType = SetType.WorkingSet, WeightKg = 100m, Reps = 5, IsCompleted = true },
                        new() { SetNumber = 2, SetType = SetType.WorkingSet, WeightKg = 100m, Reps = 5, IsCompleted = true },
                    }
                }
            }
        };

        var id = await TestApp.SendAsync(command);

        var session = await TestApp.FindAsync<WorkoutSession>(id);
        session.ShouldNotBeNull();
        session!.Notes.ShouldBe("Test workout");
        session.Status.ShouldBe(WorkoutStatus.Draft);
        session.UserId.ShouldBe(userId);
    }

    [Test]
    public async Task ShouldSaveAsDraftByDefault()
    {
        await TestApp.RunAsDefaultUserAsync();

        var exercise = new Exercise
        {
            Name = "Draft Test Exercise",
            MuscleGroup = MuscleGroup.Chest,
            EquipmentType = EquipmentType.Barbell,
            MovementPattern = MovementPattern.Push,
            IsCustom = false,
            IsActive = true
        };
        await TestApp.AddAsync(exercise);

        var command = new LogWorkoutCommand
        {
            Date = DateTimeOffset.UtcNow,
            Complete = false,
            Exercises = new List<LogWorkoutExerciseDto>
            {
                new()
                {
                    ExerciseId = exercise.Id,
                    OrderIndex = 0,
                    Sets = new List<LogWorkoutSetDto>
                    {
                        new() { SetNumber = 1, SetType = SetType.WorkingSet, WeightKg = 80m, Reps = 5, IsCompleted = false }
                    }
                }
            }
        };

        var id = await TestApp.SendAsync(command);
        var session = await TestApp.FindAsync<WorkoutSession>(id);

        session.ShouldNotBeNull();
        session!.Status.ShouldBe(WorkoutStatus.Draft);
    }
}
