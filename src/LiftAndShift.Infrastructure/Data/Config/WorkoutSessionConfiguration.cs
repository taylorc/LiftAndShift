using LiftAndShift.Core.Workouts;
using LiftAndShift.Core.WorkoutSessionAggregate;

namespace LiftAndShift.Infrastructure.Data.Config;

public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
  public void Configure(EntityTypeBuilder<WorkoutSession> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenIdValueGenerator<AppDbContext, WorkoutSession, WorkoutSessionId>>()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.FamilyMemberId)
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.Workout)
      .HasSmartEnumConversion()
      .IsRequired();

    builder.Property(entity => entity.TrainingPhase)
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.PerformedOn)
      .IsRequired();

    builder.Navigation(entity => entity.LoggedSets)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(entity => entity.LoggedSets)
      .WithOne()
      .HasForeignKey("WorkoutSessionId")
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);
  }
}
