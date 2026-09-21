using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.UseCases.PersonalRecords;
public record PersonalRecordDto(Lift Lift, WeightKg WeightKg, DateOnly AchievedOn, WorkoutSessionId WorkoutSessionId);
