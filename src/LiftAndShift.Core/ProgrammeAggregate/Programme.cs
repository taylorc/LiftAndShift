using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.Core.ProgrammeAggregate;

public class Programme(FamilyMemberId familyMemberId, TrainingPhase trainingPhase) : EntityBase<Programme, ProgrammeId>, IAggregateRoot
{
  public FamilyMemberId FamilyMemberId { get; private set; } = familyMemberId;
  public TrainingPhase TrainingPhase { get; private set; } = trainingPhase;

  public Result AdvancePhase()
  {
    if (TrainingPhase.IsAtMax)
    {
      return Result.Invalid(new ValidationError
      {
        Identifier = nameof(TrainingPhase),
        ErrorMessage = "Training Phase is already at the final phase and cannot be advanced further."
      });
    }

    TrainingPhase = TrainingPhase.Next();
    return Result.Success();
  }
}
