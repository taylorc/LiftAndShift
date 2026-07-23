using LiftAndShift.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiftAndShift.Infrastructure.Data.Configurations;

public class PersonalRecordConfiguration : IEntityTypeConfiguration<PersonalRecord>
{
    public void Configure(EntityTypeBuilder<PersonalRecord> builder)
    {
        builder.Property(pr => pr.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(pr => pr.WeightKg)
            .HasColumnType("numeric(8,3)");

        builder.Property(pr => pr.Estimated1RmKg)
            .HasColumnType("numeric(8,3)");

        builder.HasIndex(pr => new { pr.UserId, pr.ExerciseId }).IsUnique();
    }
}
