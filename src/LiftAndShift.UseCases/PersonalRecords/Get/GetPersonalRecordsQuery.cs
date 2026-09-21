using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.PersonalRecords.Get;

public record GetPersonalRecordsQuery(FamilyMemberId FamilyMemberId) : IQuery<Result<IReadOnlyList<PersonalRecordDto>>>;
