using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.UseCases.WorkoutSessions;
public record LoggedSetDto(LoggedSetId Id, Lift Lift, WeightKg WeightKg, int SetNumber, int RepsAchieved);
