using LiftAndShift.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiftAndShift.Infrastructure.Data.Configurations;

public class UserProgrammeConfiguration : IEntityTypeConfiguration<UserProgramme>
{
    public void Configure(EntityTypeBuilder<UserProgramme> builder)
    {
        builder.Property(p => p.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(p => p.ProgrammeTemplateId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.UserId);

        builder.HasMany(p => p.Sessions)
            .WithOne(s => s.UserProgramme)
            .HasForeignKey(s => s.UserProgrammeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
