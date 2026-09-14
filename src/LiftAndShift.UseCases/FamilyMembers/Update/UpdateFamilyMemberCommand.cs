using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.FamilyMembers.Update;

public record UpdateFamilyMemberCommand(FamilyMemberId FamilyMemberId, FamilyMemberName NewName, Pin NewPin) : ICommand<Result<FamilyMemberDto>>;
