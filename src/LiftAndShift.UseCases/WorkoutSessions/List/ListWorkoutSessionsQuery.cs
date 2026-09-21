using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.WorkoutSessions.List;

public record ListWorkoutSessionsQuery(FamilyMemberId FamilyMemberId, int? Page = 1, int? PerPage = Constants.DEFAULT_PAGE_SIZE)
  : IQuery<Result<PagedResult<WorkoutSessionSummaryDto>>>;
