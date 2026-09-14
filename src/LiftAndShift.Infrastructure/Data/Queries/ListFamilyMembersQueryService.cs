using LiftAndShift.UseCases.FamilyMembers;
using LiftAndShift.UseCases.FamilyMembers.List;

namespace LiftAndShift.Infrastructure.Data.Queries;

public class ListFamilyMembersQueryService : IListFamilyMembersQueryService
{
  // You can use EF, Dapper, SqlClient, etc. for queries
  private readonly AppDbContext _db;

  public ListFamilyMembersQueryService(AppDbContext db)
  {
    _db = db;
  }

  public async Task<UseCases.PagedResult<FamilyMemberDto>> ListAsync(int page, int perPage)
  {
    var items = await _db.FamilyMembers
      .OrderBy(f => f.Id)
      .Skip((page - 1) * perPage)
      .Take(perPage)
      .Select(f => new FamilyMemberDto(f.Id, f.Name, f.Pin))
      .AsNoTracking()
      .ToListAsync();

    int totalCount = await _db.FamilyMembers.CountAsync();
    int totalPages = (int)Math.Ceiling(totalCount / (double)perPage);
    var result = new UseCases.PagedResult<FamilyMemberDto>(items, page, perPage, totalCount, totalPages);

    return result;
  }
}
