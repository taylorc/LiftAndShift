using LiftAndShift.UseCases.BodyMetrics;

namespace LiftAndShift.Web.BodyMetrics;

public record BodyMetricRecord(int Id, int FamilyMemberId, DateOnly RecordedOn, decimal BodyWeightKg)
{
  public static BodyMetricRecord FromDto(BodyMetricDto e) =>
    new(e.Id.Value, e.FamilyMemberId.Value, e.RecordedOn, e.BodyWeightKg.Value);
}
