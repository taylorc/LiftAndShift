namespace LiftAndShift.UnitTests.Core.ProgrammeAggregate;

public class ProgrammeAdvancePhase
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);

  private Programme CreateProgrammeAt(int trainingPhase)
  {
    return new(_testFamilyMemberId, TrainingPhase.From(trainingPhase));
  }

  [Fact]
  public void MovesFromPhase1ToPhase2()
  {
    var programme = CreateProgrammeAt(1);

    var result = programme.AdvancePhase();

    result.IsSuccess.ShouldBeTrue();
    programme.TrainingPhase.ShouldBe(TrainingPhase.From(2));
  }

  [Fact]
  public void MovesFromPhase2ToPhase3()
  {
    var programme = CreateProgrammeAt(2);

    var result = programme.AdvancePhase();

    result.IsSuccess.ShouldBeTrue();
    programme.TrainingPhase.ShouldBe(TrainingPhase.From(3));
  }

  [Fact]
  public void FailsAndDoesNotChangePhaseWhenAlreadyAtPhase3()
  {
    var programme = CreateProgrammeAt(3);

    var result = programme.AdvancePhase();

    result.IsSuccess.ShouldBeFalse();
    programme.TrainingPhase.ShouldBe(TrainingPhase.From(3));
  }
}
