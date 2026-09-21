using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.Core.WorkWeightAggregate.Specifications;

namespace LiftAndShift.UseCases.WorkWeights.WarmUp;

public class GetWarmUpSetsHandler(IReadRepository<WorkWeight> _repository)
  : IQueryHandler<GetWarmUpSetsQuery, Result<IReadOnlyList<WarmUpSet>>>
{
  public async ValueTask<Result<IReadOnlyList<WarmUpSet>>> Handle(GetWarmUpSetsQuery request, CancellationToken cancellationToken)
  {
    var spec = new WorkWeightByFamilyMemberIdAndLiftSpec(request.FamilyMemberId, request.Lift);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null) return Result.NotFound();

    return Result.Success(entity.CalculateWarmUpSets());
  }
}
