using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LiftAndShift.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.PreferredUnit)
            .HasConversion(new ValueConverter<WeightUnit, int>(
                v => v.Value,
                v => WeightUnit.FromValue(v)));

        builder.Property(u => u.AlternatingLift)
            .HasConversion(new ValueConverter<AlternatingLiftType, int>(
                v => v.Value,
                v => AlternatingLiftType.FromValue(v)));
    }
}
