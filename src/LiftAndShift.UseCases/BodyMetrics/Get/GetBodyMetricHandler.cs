using LiftAndShift.Core.BodyMetricAggregate;
using LiftAndShift.Core.BodyMetricAggregate.Specifications;

namespace LiftAndShift.UseCases.BodyMetrics.Get;

public class GetBodyMetricHandler(IReadRepository<BodyMetric> _repository)
  : IQueryHandler<GetBodyMetricQuery, Result<BodyMetricDto>>
{
  public async ValueTask<Result<BodyMetricDto>> Handle(GetBodyMetricQuery request, CancellationToken cancellationToken)
  {
    var spec = new BodyMetricByFamilyMemberIdAndIdSpec(request.FamilyMemberId, request.BodyMetricId);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null) return Result.NotFound();

    return BodyMetricDto.FromEntity(entity);
  }
}
