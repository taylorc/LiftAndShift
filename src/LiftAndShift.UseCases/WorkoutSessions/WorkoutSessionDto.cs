using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.Workouts;
using LiftAndShift.Core.WorkoutSessionAggregate;

namespace LiftAndShift.UseCases.WorkoutSessions;
public record WorkoutSessionDto(
  WorkoutSessionId Id,
  FamilyMemberId FamilyMemberId,
  Workout Workout,
  TrainingPhase TrainingPhase,
  DateOnly PerformedOn,
  IReadOnlyList<LoggedSetDto> LoggedSets);
