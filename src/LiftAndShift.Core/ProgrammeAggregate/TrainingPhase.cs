using Vogen;

namespace LiftAndShift.Core.ProgrammeAggregate;

[ValueObject<int>(conversions: Conversions.SystemTextJson)]
public readonly partial struct TrainingPhase
{
  public const int Min = 1;
  public const int Max = 3;

  private static Validation Validate(int value)
      => value is >= Min and <= Max
        ? Validation.Ok
        : Validation.Invalid($"Training Phase must be between {Min} and {Max}.");

  public bool IsAtMax => Value == Max;

  public TrainingPhase Next() => From(Value + 1);
}
