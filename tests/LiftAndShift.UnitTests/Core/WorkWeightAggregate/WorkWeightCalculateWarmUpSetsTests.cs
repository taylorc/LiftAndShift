namespace LiftAndShift.UnitTests.Core.WorkWeightAggregate;

public class WorkWeightCalculateWarmUpSetsTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);

  private WorkWeight WorkWeightAt(Lift lift, decimal weightKg) =>
    new(_testFamilyMemberId, lift, WeightKg.From(weightKg));

  [Fact]
  public void ReturnsFourSetsForASquatAt100Kg()
  {
    var workWeight = WorkWeightAt(Lift.Squat, 100);

    var warmUpSets = workWeight.CalculateWarmUpSets();

    warmUpSets.ShouldBe(
    [
      new WarmUpSet(WeightKg.From(20), 5),
      new WarmUpSet(WeightKg.From(40), 5),
      new WarmUpSet(WeightKg.From(60), 3),
      new WarmUpSet(WeightKg.From(80), 2)
    ]);
  }

  [Fact]
  public void RoundsEachSetDownToTheNearestWholeKg()
  {
    var workWeight = WorkWeightAt(Lift.Squat, 57);

    var warmUpSets = workWeight.CalculateWarmUpSets();

    warmUpSets[1].WeightKg.ShouldBe(WeightKg.From(22)); // floor(57 * 0.4) = 22.8 -> 22
    warmUpSets[2].WeightKg.ShouldBe(WeightKg.From(34)); // floor(57 * 0.6) = 34.2 -> 34
    warmUpSets[3].WeightKg.ShouldBe(WeightKg.From(45)); // floor(57 * 0.8) = 45.6 -> 45
  }

  [Fact]
  public void NeverGoesBelowTheLiftsOpeningWeightForALowWorkWeight()
  {
    var workWeight = WorkWeightAt(Lift.Squat, 30);

    var warmUpSets = workWeight.CalculateWarmUpSets();

    warmUpSets[0].WeightKg.ShouldBe(WeightKg.From(20)); // opening weight
    warmUpSets[1].WeightKg.ShouldBe(WeightKg.From(20)); // floor(30 * 0.4) = 12, floored up to 20
    warmUpSets[2].WeightKg.ShouldBe(WeightKg.From(20)); // floor(30 * 0.6) = 18, floored up to 20
    warmUpSets[3].WeightKg.ShouldBe(WeightKg.From(24)); // floor(30 * 0.8) = 24, above the floor
  }

  [Fact]
  public void UsesDeadliftsHigherOpeningWeightAsItsFloor()
  {
    var workWeight = WorkWeightAt(Lift.Deadlift, 100);

    var warmUpSets = workWeight.CalculateWarmUpSets();

    warmUpSets[0].WeightKg.ShouldBe(WeightKg.From(70)); // opening weight
    warmUpSets[1].WeightKg.ShouldBe(WeightKg.From(70)); // floor(100 * 0.4) = 40, floored up to 70
    warmUpSets[2].WeightKg.ShouldBe(WeightKg.From(70)); // floor(100 * 0.6) = 60, floored up to 70
    warmUpSets[3].WeightKg.ShouldBe(WeightKg.From(80)); // floor(100 * 0.8) = 80, above the floor
  }

  [Fact]
  public void HasTheExpectedRepScheme()
  {
    var workWeight = WorkWeightAt(Lift.Squat, 100);

    var warmUpSets = workWeight.CalculateWarmUpSets();

    warmUpSets.Select(s => s.Reps).ShouldBe([5, 5, 3, 2]);
  }
}
