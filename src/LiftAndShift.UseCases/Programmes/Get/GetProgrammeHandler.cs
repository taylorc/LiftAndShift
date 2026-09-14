using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.ProgrammeAggregate.Specifications;

namespace LiftAndShift.UseCases.Programmes.Get;

public class GetProgrammeHandler(IReadRepository<Programme> _repository)
  : IQueryHandler<GetProgrammeQuery, Result<ProgrammeDto>>
{
  public async ValueTask<Result<ProgrammeDto>> Handle(GetProgrammeQuery request, CancellationToken cancellationToken)
  {
    var spec = new ProgrammeByFamilyMemberIdSpec(request.FamilyMemberId);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null) return Result.NotFound();

    return new ProgrammeDto(entity.Id, entity.FamilyMemberId, entity.TrainingPhase);
  }
}
