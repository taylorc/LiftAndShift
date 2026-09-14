using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.FamilyMembers.Create;

/// <summary>
/// Create a new Family Member.
/// </summary>
public record CreateFamilyMemberCommand(FamilyMemberName Name, Pin Pin) : ICommand<Result<FamilyMemberId>>;
