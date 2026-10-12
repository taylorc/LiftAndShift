using LiftAndShift.Core.BodyMetricAggregate;
using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.BodyMetrics.Get;

public record GetBodyMetricQuery(FamilyMemberId FamilyMemberId, BodyMetricId BodyMetricId) : IQuery<Result<BodyMetricDto>>;
