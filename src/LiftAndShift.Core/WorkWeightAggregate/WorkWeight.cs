using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.Core.WorkWeightAggregate;

public class WorkWeight(FamilyMemberId familyMemberId, Lift lift, WeightKg weightKg) : EntityBase<WorkWeight, WorkWeightId>, IAggregateRoot
{
  public FamilyMemberId FamilyMemberId { get; private set; } = familyMemberId;
  public Lift Lift { get; private set; } = lift;
  public WeightKg WeightKg { get; private set; } = weightKg;

  public static WorkWeight CompleteRamp(FamilyMemberId familyMemberId, Lift lift, int finalSetNumber) =>
    new(familyMemberId, lift, WeightKg.From(lift.RampWeightForSet(finalSetNumber)));
}
