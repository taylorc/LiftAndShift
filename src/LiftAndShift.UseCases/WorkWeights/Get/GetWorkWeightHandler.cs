using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.Core.WorkWeightAggregate.Specifications;

namespace LiftAndShift.UseCases.WorkWeights.Get;

public class GetWorkWeightHandler(IReadRepository<WorkWeight> _repository)
  : IQueryHandler<GetWorkWeightQuery, Result<WorkWeightDto>>
{
  public async ValueTask<Result<WorkWeightDto>> Handle(GetWorkWeightQuery request, CancellationToken cancellationToken)
  {
    var spec = new WorkWeightByFamilyMemberIdAndLiftSpec(request.FamilyMemberId, request.Lift);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null) return Result.NotFound();

    return new WorkWeightDto(entity.Id, entity.FamilyMemberId, entity.Lift, entity.WeightKg, entity.ConsecutiveFailures);
  }
}
