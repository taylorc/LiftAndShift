namespace LiftAndShift.UseCases.FamilyMembers.List;

public record ListFamilyMembersQuery(int? Page = 1, int? PerPage = Constants.DEFAULT_PAGE_SIZE)
  : IQuery<Result<PagedResult<FamilyMemberDto>>>;
