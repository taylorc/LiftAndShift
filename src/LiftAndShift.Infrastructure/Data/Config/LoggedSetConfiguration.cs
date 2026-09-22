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
      .HasSmartEnumConversion()
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
