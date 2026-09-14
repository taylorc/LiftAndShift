using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.Programmes.AdvancePhase;

public record AdvanceTrainingPhaseCommand(FamilyMemberId FamilyMemberId) : ICommand<Result<ProgrammeDto>>;
