using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.Workouts;
using LiftAndShift.Core.WorkoutSessionAggregate;

namespace LiftAndShift.UseCases.WorkoutSessions;
public record WorkoutSessionSummaryDto(
  WorkoutSessionId Id,
  FamilyMemberId FamilyMemberId,
  Workout Workout,
  TrainingPhase TrainingPhase,
  DateOnly PerformedOn);
