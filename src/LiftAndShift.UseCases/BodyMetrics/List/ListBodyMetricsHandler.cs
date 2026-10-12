namespace LiftAndShift.UseCases.BodyMetrics.List;

public class ListBodyMetricsHandler(IListBodyMetricsQueryService _query)
  : IQueryHandler<ListBodyMetricsQuery, Result<PagedResult<BodyMetricDto>>>
{
  public async ValueTask<Result<PagedResult<BodyMetricDto>>> Handle(ListBodyMetricsQuery request,
                                                                     CancellationToken cancellationToken)
  {
    var result = await _query.ListAsync(request.FamilyMemberId, request.Page ?? 1, request.PerPage ?? Constants.DEFAULT_PAGE_SIZE);

    return Result.Success(result);
  }
}
