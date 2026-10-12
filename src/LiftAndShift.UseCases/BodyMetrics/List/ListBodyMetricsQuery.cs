using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.BodyMetrics.List;

public record ListBodyMetricsQuery(FamilyMemberId FamilyMemberId, int? Page = 1, int? PerPage = Constants.DEFAULT_PAGE_SIZE)
  : IQuery<Result<PagedResult<BodyMetricDto>>>;
