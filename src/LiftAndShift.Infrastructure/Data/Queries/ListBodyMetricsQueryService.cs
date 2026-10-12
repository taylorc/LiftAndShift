using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.BodyMetrics;
using LiftAndShift.UseCases.BodyMetrics.List;

namespace LiftAndShift.Infrastructure.Data.Queries;

public class ListBodyMetricsQueryService(AppDbContext db) : IListBodyMetricsQueryService
{
  public async Task<UseCases.PagedResult<BodyMetricDto>> ListAsync(FamilyMemberId familyMemberId, int page, int perPage)
  {
    var query = db.BodyMetrics.Where(metric => metric.FamilyMemberId == familyMemberId);

    var items = await query
      .OrderByDescending(metric => metric.RecordedOn)
      .ThenByDescending(metric => metric.Id)
      .Skip((page - 1) * perPage)
      .Take(perPage)
      .Select(metric => new BodyMetricDto(metric.Id, metric.FamilyMemberId, metric.RecordedOn, metric.BodyWeightKg))
      .AsNoTracking()
      .ToListAsync();

    int totalCount = await query.CountAsync();
    int totalPages = (int)Math.Ceiling(totalCount / (double)perPage);
    var result = new UseCases.PagedResult<BodyMetricDto>(items, page, perPage, totalCount, totalPages);

    return result;
  }
}
