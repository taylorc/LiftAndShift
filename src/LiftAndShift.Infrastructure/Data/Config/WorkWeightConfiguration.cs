using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.Infrastructure.Data.Config;

public class WorkWeightConfiguration : IEntityTypeConfiguration<WorkWeight>
{
  public void Configure(EntityTypeBuilder<WorkWeight> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenIdValueGenerator<AppDbContext, WorkWeight, WorkWeightId>>()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.FamilyMemberId)
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.Lift)
      .HasSmartEnumConversion()
      .IsRequired();

    builder.Property(entity => entity.WeightKg)
      .HasVogenConversion()
      .IsRequired();

    builder.HasIndex(entity => new { entity.FamilyMemberId, entity.Lift })
      .IsUnique();
  }
}
