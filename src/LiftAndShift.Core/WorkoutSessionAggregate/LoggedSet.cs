using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.Core.WorkoutSessionAggregate;

public class LoggedSet(Lift lift, WeightKg weightKg, int setNumber, int repsAchieved) : EntityBase<LoggedSet, LoggedSetId>
{
  public Lift Lift { get; private set; } = lift;
  public WeightKg WeightKg { get; private set; } = weightKg;
  public int SetNumber { get; private set; } = setNumber;
  public int RepsAchieved { get; private set; } = Guard.Against.Negative(repsAchieved, nameof(repsAchieved));
}
