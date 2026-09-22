using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.FamilyMemberAggregate.Specifications;

namespace LiftAndShift.UseCases.PersonalRecords.Get;

public class GetPersonalRecordsHandler(
  IReadRepository<FamilyMember> _familyMemberRepository,
  IPersonalRecordsQueryService _query)
  : IQueryHandler<GetPersonalRecordsQuery, Result<IReadOnlyList<PersonalRecordDto>>>
{
  public async ValueTask<Result<IReadOnlyList<PersonalRecordDto>>> Handle(GetPersonalRecordsQuery request, CancellationToken cancellationToken)
  {
    var familyMember = await _familyMemberRepository.FirstOrDefaultAsync(
      new FamilyMemberByIdSpec(request.FamilyMemberId), cancellationToken);
    if (familyMember == null) return Result.NotFound();

    var records = await _query.GetAsync(request.FamilyMemberId, cancellationToken);

    return Result.Success(records);
  }
}
