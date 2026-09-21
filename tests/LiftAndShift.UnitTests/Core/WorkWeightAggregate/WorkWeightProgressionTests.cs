namespace LiftAndShift.UnitTests.Core.WorkWeightAggregate;

public class WorkWeightProgressionTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);

  [Fact]
  public void RecordSuccessIncreasesWeightByProgressionIncrementForA5kgLift()
  {
    var workWeight = WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, finalSetNumber: 3); // 30kg

    workWeight.RecordSuccess();

    workWeight.WeightKg.ShouldBe(WeightKg.From(35));
    workWeight.ConsecutiveFailures.ShouldBe(0);
  }

  [Fact]
  public void RecordSuccessIncreasesWeightByProgressionIncrementForA2Point5kgLift()
  {
    var workWeight = WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Press, finalSetNumber: 3); // 30kg

    workWeight.RecordSuccess();

    workWeight.WeightKg.ShouldBe(WeightKg.From(32.5m));
  }

  [Fact]
  public void RecordSuccessResetsAnExistingFailureStreak()
  {
    var workWeight = WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, finalSetNumber: 3);
    workWeight.RecordFailure();

    workWeight.RecordSuccess();

    workWeight.ConsecutiveFailures.ShouldBe(0);
  }

  [Fact]
  public void FirstFailureOnlyMarksTheStreakAndLeavesWeightUnchanged()
  {
    var workWeight = WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, finalSetNumber: 3); // 30kg

    bool deloaded = workWeight.RecordFailure();

    deloaded.ShouldBeFalse();
    workWeight.WeightKg.ShouldBe(WeightKg.From(30));
    workWeight.ConsecutiveFailures.ShouldBe(1);
  }

  [Fact]
  public void SecondConsecutiveFailureDeloadsByTenPercentRoundedDownAndResetsStreak()
  {
    var workWeight = WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, finalSetNumber: 3); // 30kg
    workWeight.RecordFailure();

    bool deloaded = workWeight.RecordFailure();

    deloaded.ShouldBeTrue();
    workWeight.WeightKg.ShouldBe(WeightKg.From(27)); // floor(30 * 0.9) = 27
    workWeight.ConsecutiveFailures.ShouldBe(0);
  }

  [Fact]
  public void ASuccessBetweenTwoFailuresPreventsADeload()
  {
    var workWeight = WorkWeight.CompleteRamp(_testFamilyMemberId, Lift.Squat, finalSetNumber: 3); // 30kg
    workWeight.RecordFailure(); // streak = 1
    workWeight.RecordSuccess(); // streak reset, weight = 35

    bool deloaded = workWeight.RecordFailure(); // only the "first" failure again

    deloaded.ShouldBeFalse();
    workWeight.WeightKg.ShouldBe(WeightKg.From(35));
    workWeight.ConsecutiveFailures.ShouldBe(1);
  }

  [Fact]
  public void ADeloadNeverDropsBelowTheLiftsOpeningWeight()
  {
    var workWeight = new WorkWeight(_testFamilyMemberId, Lift.Squat, WeightKg.From(Lift.Squat.OpeningWeightKg)); // 20kg
    workWeight.RecordFailure(); // streak = 1

    bool deloaded = workWeight.RecordFailure(); // would compute floor(20 * 0.9) = 18, but floors at 20

    deloaded.ShouldBeTrue();
    workWeight.WeightKg.ShouldBe(WeightKg.From(20));
    workWeight.ConsecutiveFailures.ShouldBe(0);
  }

  [Fact]
  public void ADeloadThatWouldCrossTheFloorLandsExactlyOnIt()
  {
    var workWeight = new WorkWeight(_testFamilyMemberId, Lift.Squat, WeightKg.From(21)); // just above the 20kg floor
    workWeight.RecordFailure(); // streak = 1

    workWeight.RecordFailure(); // would compute floor(21 * 0.9) = 18, but floors at 20

    workWeight.WeightKg.ShouldBe(WeightKg.From(20));
  }
}
