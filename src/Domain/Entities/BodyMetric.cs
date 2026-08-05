namespace LiftAndShift.Domain.Entities;

public class BodyMetric : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;

    public DateTimeOffset Date { get; set; }

    public decimal WeightKg { get; set; }

    public string? Notes { get; set; }
}
