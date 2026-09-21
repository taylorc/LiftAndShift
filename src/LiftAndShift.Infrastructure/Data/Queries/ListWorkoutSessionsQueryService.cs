using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.WorkoutSessions;
using LiftAndShift.UseCases.WorkoutSessions.List;

namespace LiftAndShift.Infrastructure.Data.Queries;

public class ListWorkoutSessionsQueryService(AppDbContext db) : IListWorkoutSessionsQueryService
{
  public async Task<UseCases.PagedResult<WorkoutSessionSummaryDto>> ListAsync(FamilyMemberId familyMemberId, int page, int perPage)
  {
    var query = db.WorkoutSessions.Where(session => session.FamilyMemberId == familyMemberId);

    var items = await query
      .OrderByDescending(session => session.PerformedOn)
      .ThenByDescending(session => session.Id)
      .Skip((page - 1) * perPage)
      .Take(perPage)
      .Select(session => new WorkoutSessionSummaryDto(session.Id, session.FamilyMemberId, session.Workout, session.TrainingPhase, session.PerformedOn))
      .AsNoTracking()
      .ToListAsync();

    int totalCount = await query.CountAsync();
    int totalPages = (int)Math.Ceiling(totalCount / (double)perPage);
    var result = new UseCases.PagedResult<WorkoutSessionSummaryDto>(items, page, perPage, totalCount, totalPages);

    return result;
  }
}
