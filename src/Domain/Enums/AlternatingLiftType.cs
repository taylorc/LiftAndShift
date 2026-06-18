using Ardalis.SmartEnum;

namespace LiftAndShift.Domain.Enums;

public sealed class AlternatingLiftType : SmartEnum<AlternatingLiftType>
{
    public static readonly AlternatingLiftType PowerClean = new(nameof(PowerClean), 0);
    public static readonly AlternatingLiftType PendlayRow = new(nameof(PendlayRow), 1);

    private AlternatingLiftType(string name, int value) : base(name, value) { }
}
