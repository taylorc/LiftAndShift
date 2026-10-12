using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.Core.BodyMetricAggregate;

/// <summary>
/// A single bodyweight entry in a Family Member's append-only body metrics history. Nothing reads or
/// reacts to it yet, and there's deliberately no cross-entry rule - several entries on the same date
/// (e.g. a re-weigh) are all kept.
/// </summary>
public class BodyMetric(FamilyMemberId familyMemberId, DateOnly recordedOn, WeightKg bodyWeightKg)
  : EntityBase<BodyMetric, BodyMetricId>, IAggregateRoot
{
  public FamilyMemberId FamilyMemberId { get; private set; } = familyMemberId;
  public DateOnly RecordedOn { get; private set; } = recordedOn;
  public WeightKg BodyWeightKg { get; private set; } = bodyWeightKg;
}
