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
  /// (rounded down to the nearest whole kg, never below the lift's opening weight) and resets the
  /// streak. Returns whether this call deloaded (the rule fired, even if the floor left the weight
  /// unchanged) - not whether the weight actually decreased.
  /// </summary>
  public bool RecordFailure()
  {
    if (ConsecutiveFailures >= 1)
    {
      WeightKg = WeightKg.From(Math.Max(Lift.OpeningWeightKg, Math.Floor(WeightKg.Value * DeloadMultiplier)));
      ConsecutiveFailures = 0;
      return true;
    }

    ConsecutiveFailures = 1;
    return false;
  }

  /// <summary>
  /// The fixed four-set warm-up ladder leading up to (but not including) this Work Weight, at 5/5/3/2
  /// reps respectively: the lift's opening weight, then 40%, 60%, and 80% of Work Weight, each rounded
  /// down to the nearest whole kg and never below the lift's opening weight.
  /// </summary>
  public IReadOnlyList<WarmUpSet> CalculateWarmUpSets() =>
  [
    new WarmUpSet(WeightKg.From(Lift.OpeningWeightKg), 5),
    WarmUpSetAtPercentage(0.4m, 5),
    WarmUpSetAtPercentage(0.6m, 3),
    WarmUpSetAtPercentage(0.8m, 2)
  ];

  private WarmUpSet WarmUpSetAtPercentage(decimal percentage, int reps)
  {
    var weightKg = Math.Max(Lift.OpeningWeightKg, Math.Floor(WeightKg.Value * percentage));
    return new WarmUpSet(WeightKg.From(weightKg), reps);
  }
}
