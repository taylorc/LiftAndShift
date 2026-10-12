using LiftAndShift.Core.BodyMetricAggregate;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.UseCases.BodyMetrics;

public record BodyMetricDto(
  BodyMetricId Id,
  FamilyMemberId FamilyMemberId,
  DateOnly RecordedOn,
  WeightKg BodyWeightKg)
{
  public static BodyMetricDto FromEntity(BodyMetric entity) =>
    new(entity.Id, entity.FamilyMemberId, entity.RecordedOn, entity.BodyWeightKg);
}
