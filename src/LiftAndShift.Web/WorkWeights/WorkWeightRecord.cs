namespace LiftAndShift.Web.WorkWeights;

public record WorkWeightRecord(int Id, int FamilyMemberId, string Lift, decimal WeightKg, int ConsecutiveFailures);
