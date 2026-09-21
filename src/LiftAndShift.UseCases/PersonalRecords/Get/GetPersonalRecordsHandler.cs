using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.FamilyMemberAggregate.Specifications;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkoutSessionAggregate.Specifications;

namespace LiftAndShift.UseCases.PersonalRecords.Get;

public class GetPersonalRecordsHandler(
  IReadRepository<FamilyMember> _familyMemberRepository,
  IReadRepository<WorkoutSession> _workoutSessionRepository)
  : IQueryHandler<GetPersonalRecordsQuery, Result<IReadOnlyList<PersonalRecordDto>>>
{
  public async ValueTask<Result<IReadOnlyList<PersonalRecordDto>>> Handle(GetPersonalRecordsQuery request, CancellationToken cancellationToken)
  {
    var familyMember = await _familyMemberRepository.FirstOrDefaultAsync(
      new FamilyMemberByIdSpec(request.FamilyMemberId), cancellationToken);
    if (familyMember == null) return Result.NotFound();

    var sessions = await _workoutSessionRepository.ListAsync(
      new WorkoutSessionsByFamilyMemberIdSpec(request.FamilyMemberId), cancellationToken);

    return Result.Success(PersonalRecordCalculator.Calculate(sessions));
  }
}
