using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.Core.WorkoutSessionAggregate;

public record LoggedLiftInput(Lift Lift, WeightKg WeightKg, IReadOnlyList<int> RepsPerSet);
