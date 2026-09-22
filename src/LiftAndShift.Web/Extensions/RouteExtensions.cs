using System.Text.RegularExpressions;

namespace LiftAndShift.Web.Extensions;

public static partial class RouteExtensions
{
  /// <summary>
  /// Fills a FastEndpoints route template's {Token} placeholders, in declaration order, with the given
  /// values - e.g. "/FamilyMembers/{FamilyMemberId:int}/WorkWeights/{Lift}".BuildRoute(1, "Squat").
  /// One place to get route-templating right instead of a hand-rolled Replace() chain per request type.
  /// </summary>
  public static string BuildRoute(this string routeTemplate, params object[] values)
  {
    int i = 0;
    return RouteTokenPattern().Replace(routeTemplate, _ => values[i++].ToString()!);
  }

  [GeneratedRegex(@"\{[^/}]+\}")]
  private static partial Regex RouteTokenPattern();
}
