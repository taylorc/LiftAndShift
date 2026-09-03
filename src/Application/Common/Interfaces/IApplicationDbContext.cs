using LiftAndShift.Domain.Entities;

namespace LiftAndShift.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Exercise> Exercises { get; }

    DbSet<ExerciseCategory> ExerciseCategories { get; }

    DbSet<WorkoutSession> WorkoutSessions { get; }

    DbSet<WorkoutExercise> WorkoutExercises { get; }

    DbSet<WorkoutSet> WorkoutSets { get; }

    DbSet<UserProgramme> UserProgrammes { get; }

    DbSet<ProgrammeSession> ProgrammeSessions { get; }

    DbSet<BodyMetric> BodyMetrics { get; }

    DbSet<PersonalRecord> PersonalRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
