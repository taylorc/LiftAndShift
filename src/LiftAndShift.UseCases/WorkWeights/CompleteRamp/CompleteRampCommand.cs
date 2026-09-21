using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.UseCases.WorkWeights.CompleteRamp;

public record CompleteRampCommand(FamilyMemberId FamilyMemberId, Lift Lift, int FinalSetNumber) : ICommand<Result<WorkWeightDto>>;
