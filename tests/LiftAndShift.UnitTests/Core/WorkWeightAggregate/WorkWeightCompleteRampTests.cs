namespace LiftAndShift.UnitTests.Core.WorkWeightAggregate;

public class WorkWeightCompleteRampTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);

  [Fact]
  public void InitializesFamilyMemberIdLiftAndWeight()
  {
    var workWeight = WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, finalSetNumber: 3);

    workWeight.FamilyMemberId.ShouldBe(_testFamilyMemberId);
    workWeight.Lift.ShouldBe(Lift.Squat);
    workWeight.WeightKg.ShouldBe(WeightKg.From(30));
  }

  [Fact]
  public void ComputesDeadliftWeightWithItsOwnIncrement()
  {
    var workWeight = WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Deadlift, finalSetNumber: 3);

    workWeight.WeightKg.ShouldBe(WeightKg.From(90));
  }
}
