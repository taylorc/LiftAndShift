namespace LiftAndShift.UnitTests.Core.ProgrammeAggregate;

public class ProgrammeConstructorTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly TrainingPhase _testTrainingPhase = TrainingPhase.From(1);

  private Programme CreateProgramme()
  {
    return new(_testFamilyMemberId, _testTrainingPhase);
  }

  [Fact]
  public void InitializesFamilyMemberId()
  {
    var programme = CreateProgramme();

    programme.FamilyMemberId.ShouldBe(_testFamilyMemberId);
  }

  [Fact]
  public void InitializesTrainingPhase()
  {
    var programme = CreateProgramme();

    programme.TrainingPhase.ShouldBe(_testTrainingPhase);
  }
}
