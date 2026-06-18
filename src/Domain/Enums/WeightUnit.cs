using Ardalis.SmartEnum;

namespace LiftAndShift.Domain.Enums;

public sealed class WeightUnit : SmartEnum<WeightUnit>
{
    public static readonly WeightUnit Lbs = new(nameof(Lbs), 0);
    public static readonly WeightUnit Kgs = new(nameof(Kgs), 1);

    private WeightUnit(string name, int value) : base(name, value) { }
}
