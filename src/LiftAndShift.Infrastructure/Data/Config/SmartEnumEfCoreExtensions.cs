using Ardalis.SmartEnum;

namespace LiftAndShift.Infrastructure.Data.Config;

public static class SmartEnumEfCoreExtensions
{
  /// <summary>
  /// Configures a property backed by an Ardalis.SmartEnum value (e.g. Lift, Workout, ContributorStatus)
  /// to be stored as its underlying int Value, mirroring how HasVogenConversion() handles Vogen value
  /// objects - one place to fix if the conversion needs to change, instead of a hand-rolled
  /// HasConversion(x => x.Value, v => TEnum.FromValue(v)) in every entity configuration.
  /// </summary>
  public static PropertyBuilder<TEnum> HasSmartEnumConversion<TEnum>(this PropertyBuilder<TEnum> builder)
    where TEnum : SmartEnum<TEnum>
  {
    return builder.HasConversion(
      smartEnum => smartEnum.Value,
      value => SmartEnum<TEnum>.FromValue(value));
  }
}
