namespace LiftAndShift.Web.WorkoutSessions;

public record WorkoutSessionSummaryRecord(int Id, int FamilyMemberId, string Workout, int TrainingPhase, DateOnly PerformedOn);
