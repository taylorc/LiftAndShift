using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.BodyMetrics.List;

/// <summary>
/// Represents a service that will actually fetch the necessary data
/// Typically implemented in Infrastructure
/// </summary>
public interface IListBodyMetricsQueryService
{
  Task<UseCases.PagedResult<BodyMetricDto>> ListAsync(FamilyMemberId familyMemberId, int page, int perPage);
}
