using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.PersonalRecords.Get;

/// <summary>
/// Represents a service that will actually fetch the necessary data
/// Typically implemented in Infrastructure
/// </summary>
public interface IPersonalRecordsQueryService
{
  Task<IReadOnlyList<PersonalRecordDto>> GetAsync(FamilyMemberId familyMemberId, CancellationToken cancellationToken);
}
