using System.Reflection;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LiftAndShift.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Exercise> Exercises => Set<Exercise>();

    public DbSet<ExerciseCategory> ExerciseCategories => Set<ExerciseCategory>();

    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();

    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();

    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();

    public DbSet<UserProgramme> UserProgrammes => Set<UserProgramme>();

    public DbSet<ProgrammeSession> ProgrammeSessions => Set<ProgrammeSession>();

    public DbSet<BodyMetric> BodyMetrics => Set<BodyMetric>();

    public DbSet<PersonalRecord> PersonalRecords => Set<PersonalRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
