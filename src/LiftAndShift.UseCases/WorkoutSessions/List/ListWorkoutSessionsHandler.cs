namespace LiftAndShift.UseCases.WorkoutSessions.List;

public class ListWorkoutSessionsHandler(IListWorkoutSessionsQueryService _query)
  : IQueryHandler<ListWorkoutSessionsQuery, Result<PagedResult<WorkoutSessionSummaryDto>>>
{
  public async ValueTask<Result<PagedResult<WorkoutSessionSummaryDto>>> Handle(ListWorkoutSessionsQuery request,
                                                                                 CancellationToken cancellationToken)
  {
    var result = await _query.ListAsync(request.FamilyMemberId, request.Page ?? 1, request.PerPage ?? Constants.DEFAULT_PAGE_SIZE);

    return Result.Success(result);
  }
}
