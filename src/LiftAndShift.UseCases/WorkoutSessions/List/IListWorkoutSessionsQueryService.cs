using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.WorkoutSessions.List;

/// <summary>
/// Represents a service that will actually fetch the necessary data
/// Typically implemented in Infrastructure
/// </summary>
public interface IListWorkoutSessionsQueryService
{
  Task<UseCases.PagedResult<WorkoutSessionSummaryDto>> ListAsync(FamilyMemberId familyMemberId, int page, int perPage);
}
