using System.Text.Json;
using LiftAndShift.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiftAndShift.Infrastructure.Data.Configurations;

public class ProgrammeSessionConfiguration : IEntityTypeConfiguration<ProgrammeSession>
{
    public void Configure(EntityTypeBuilder<ProgrammeSession> builder)
    {
        builder.Property(s => s.LiftProgression)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, decimal>())
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, decimal>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                d => d.Aggregate(0, (hash, kvp) => HashCode.Combine(hash, kvp.Key.GetHashCode(), kvp.Value.GetHashCode())),
                d => new Dictionary<string, decimal>(d)));

        builder.Property(s => s.LiftProgression)
            .HasColumnType("jsonb");
    }
}
