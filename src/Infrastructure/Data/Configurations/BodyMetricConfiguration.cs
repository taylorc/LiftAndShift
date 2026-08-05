using LiftAndShift.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiftAndShift.Infrastructure.Data.Configurations;

public class BodyMetricConfiguration : IEntityTypeConfiguration<BodyMetric>
{
    public void Configure(EntityTypeBuilder<BodyMetric> builder)
    {
        builder.Property(m => m.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(m => m.WeightKg)
            .HasColumnType("numeric(8,3)");

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        builder.HasIndex(m => m.UserId);
    }
}
