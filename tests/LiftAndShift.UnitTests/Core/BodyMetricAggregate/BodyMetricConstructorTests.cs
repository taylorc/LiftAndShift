namespace LiftAndShift.UnitTests.Core.BodyMetricAggregate;

public class BodyMetricConstructorTests
{
  [Fact]
  public void InitializesFamilyMemberIdRecordedOnAndBodyWeight()
  {
    var familyMemberId = FamilyMemberId.From(1);
    var recordedOn = new DateOnly(2026, 10, 12);
    var bodyWeightKg = WeightKg.From(82.5m);

    var bodyMetric = new BodyMetric(familyMemberId, recordedOn, bodyWeightKg);

    bodyMetric.FamilyMemberId.ShouldBe(familyMemberId);
    bodyMetric.RecordedOn.ShouldBe(recordedOn);
    bodyMetric.BodyWeightKg.ShouldBe(bodyWeightKg);
  }
}
