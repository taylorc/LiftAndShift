using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.FamilyMembers.Delete;

public record DeleteFamilyMemberCommand(FamilyMemberId FamilyMemberId) : ICommand<Result>;
