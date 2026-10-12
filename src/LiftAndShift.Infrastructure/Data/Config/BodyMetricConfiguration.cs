using LiftAndShift.Core.BodyMetricAggregate;

namespace LiftAndShift.Infrastructure.Data.Config;

public class BodyMetricConfiguration : IEntityTypeConfiguration<BodyMetric>
{
  public void Configure(EntityTypeBuilder<BodyMetric> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenIdValueGenerator<AppDbContext, BodyMetric, BodyMetricId>>()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.FamilyMemberId)
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.RecordedOn)
      .IsRequired();

    builder.Property(entity => entity.BodyWeightKg)
      .HasVogenConversion()
      .IsRequired();
  }
}
