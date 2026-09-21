using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.UseCases.WorkoutSessions;
public record LiftOutcomeDto(Lift Lift, bool Successful, WeightKg NewWeightKg, bool Deloaded);
