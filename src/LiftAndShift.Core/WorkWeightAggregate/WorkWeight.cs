using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.Core.WorkWeightAggregate;

public class WorkWeight(FamilyMemberId familyMemberId, Lift lift, WeightKg weightKg) : EntityBase<WorkWeight, WorkWeightId>, IAggregateRoot
{
  private const decimal DeloadMultiplier = 0.9m;

  public FamilyMemberId FamilyMemberId { get; private set; } = familyMemberId;
  public Lift Lift { get; private set; } = lift;
  public WeightKg WeightKg { get; private set; } = weightKg;
  public int ConsecutiveFailures { get; private set; }

  public static WorkWeight CompleteRamp(FamilyMemberId familyMemberId, Lift lift, int finalSetNumber) =>
    new(familyMemberId, lift, WeightKg.From(lift.RampWeightForSet(finalSetNumber)));

  /// <summary>
  /// Records a session where every set for this lift hit its rep target: Work Weight climbs by the
  /// lift's Progression Increment, and any failure streak is cleared.
  /// </summary>
  public void RecordSuccess()
  {
    WeightKg = WeightKg.From(WeightKg.Value + Lift.ProgressionIncrementKg);
    ConsecutiveFailures = 0;
  }

  /// <summary>
  /// Records a session where at least one set for this lift missed its rep target. The first failure
  /// only marks the streak; the second *consecutive* failure at the same Work Weight deloads it by 10%
  /// (rounded down to the nearest whole kg) and resets the streak. Returns whether this call deloaded.
  /// </summary>
  public bool RecordFailure()
  {
    if (ConsecutiveFailures >= 1)
    {
      WeightKg = WeightKg.From(Math.Floor(WeightKg.Value * DeloadMultiplier));
      ConsecutiveFailures = 0;
      return true;
    }

    ConsecutiveFailures = 1;
    return false;
  }
}
