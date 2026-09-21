namespace LiftAndShift.Web.WorkoutSessions;

public record LiftOutcomeRecord(string Lift, bool Successful, decimal NewWeightKg, bool Deloaded);
