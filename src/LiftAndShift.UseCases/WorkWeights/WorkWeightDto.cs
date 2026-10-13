using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.UseCases.WorkWeights;
public record WorkWeightDto(WorkWeightId Id, FamilyMemberId FamilyMemberId, Lift Lift, WeightKg WeightKg, int ConsecutiveFailures)
{
  public static WorkWeightDto FromEntity(WorkWeight entity) =>
    new(entity.Id, entity.FamilyMemberId, entity.Lift, entity.WeightKg, entity.ConsecutiveFailures);
}
