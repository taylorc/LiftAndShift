namespace LiftAndShift.UseCases.FamilyMembers.List;

public class ListFamilyMembersHandler : IQueryHandler<ListFamilyMembersQuery, Result<PagedResult<FamilyMemberDto>>>
{
  private readonly IListFamilyMembersQueryService _query;

  public ListFamilyMembersHandler(IListFamilyMembersQueryService query)
  {
    _query = query;
  }

  public async ValueTask<Result<PagedResult<FamilyMemberDto>>> Handle(ListFamilyMembersQuery request,
                                                                       CancellationToken cancellationToken)
  {
    var result = await _query.ListAsync(request.Page ?? 1, request.PerPage ?? Constants.DEFAULT_PAGE_SIZE);

    return Result.Success(result);
  }
}
