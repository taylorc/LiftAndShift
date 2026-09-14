using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.FamilyMemberAggregate.Specifications;

namespace LiftAndShift.UseCases.FamilyMembers.Get;

public class GetFamilyMemberHandler(IReadRepository<FamilyMember> _repository)
  : IQueryHandler<GetFamilyMemberQuery, Result<FamilyMemberDto>>
{
  public async ValueTask<Result<FamilyMemberDto>> Handle(GetFamilyMemberQuery request, CancellationToken cancellationToken)
  {
    var spec = new FamilyMemberByIdSpec(request.FamilyMemberId);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null) return Result.NotFound();

    return new FamilyMemberDto(entity.Id, entity.Name, entity.Pin);
  }
}
