using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Workouts;
using LiftAndShift.Core.WorkoutSessionAggregate;

namespace LiftAndShift.UseCases.WorkoutSessions.Log;

public record LogWorkoutSessionCommand(
  FamilyMemberId FamilyMemberId,
  Workout Workout,
  DateOnly PerformedOn,
  IReadOnlyList<LoggedLiftInput> LoggedLifts) : ICommand<Result<WorkoutSessionDto>>;
