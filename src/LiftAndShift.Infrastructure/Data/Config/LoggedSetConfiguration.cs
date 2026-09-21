using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkoutSessionAggregate;

namespace LiftAndShift.Infrastructure.Data.Config;

public class LoggedSetConfiguration : IEntityTypeConfiguration<LoggedSet>
{
  public void Configure(EntityTypeBuilder<LoggedSet> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenIdValueGenerator<AppDbContext, LoggedSet, LoggedSetId>>()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.Lift)
      .HasConversion(
          lift => lift.Value,
          value => Lift.FromValue(value))
      .IsRequired();

    builder.Property(entity => entity.WeightKg)
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.SetNumber)
      .IsRequired();

    builder.Property(entity => entity.RepsAchieved)
      .IsRequired();
  }
}
