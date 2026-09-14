namespace LiftAndShift.UseCases.FamilyMembers.List;

/// <summary>
/// Represents a service that will actually fetch the necessary data
/// Typically implemented in Infrastructure
/// </summary>
public interface IListFamilyMembersQueryService
{
  Task<UseCases.PagedResult<FamilyMemberDto>> ListAsync(int page, int perPage);
}
