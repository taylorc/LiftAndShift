using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.Infrastructure.Data.Config;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
  public void Configure(EntityTypeBuilder<FamilyMember> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenIdValueGenerator<AppDbContext, FamilyMember, FamilyMemberId>>()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.Name)
      .HasVogenConversion()
      .HasMaxLength(FamilyMemberName.MaxLength)
      .IsRequired();

    builder.Property(entity => entity.Pin)
      .HasVogenConversion()
      .HasMaxLength(4)
      .IsRequired();
  }
}
