using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.FamilyMembers.Get;

public record GetFamilyMemberQuery(FamilyMemberId FamilyMemberId) : IQuery<Result<FamilyMemberDto>>;
