// The API returns Lift names as their C# SmartEnum identifiers (e.g. "BenchPress", "LatPulldown") -
// PascalCase with no spaces. This splits them back into words for display.
export function formatLiftName(name: string): string {
  return name.replace(/([a-z])([A-Z])/g, '$1 $2')
}
