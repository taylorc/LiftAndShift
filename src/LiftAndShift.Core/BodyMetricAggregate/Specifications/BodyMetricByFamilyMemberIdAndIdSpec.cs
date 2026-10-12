using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.Core.BodyMetricAggregate.Specifications;

public class BodyMetricByFamilyMemberIdAndIdSpec : Specification<BodyMetric>
{
  public BodyMetricByFamilyMemberIdAndIdSpec(FamilyMemberId familyMemberId, BodyMetricId bodyMetricId) =>
    Query
        .Where(metric => metric.FamilyMemberId == familyMemberId && metric.Id == bodyMetricId);
}
