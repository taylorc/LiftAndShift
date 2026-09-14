using LiftAndShift.Core.ProgrammeAggregate;

namespace LiftAndShift.Infrastructure.Data.Config;

public class ProgrammeConfiguration : IEntityTypeConfiguration<Programme>
{
  public void Configure(EntityTypeBuilder<Programme> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenIdValueGenerator<AppDbContext, Programme, ProgrammeId>>()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.FamilyMemberId)
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.TrainingPhase)
      .HasVogenConversion()
      .IsRequired();
  }
}
