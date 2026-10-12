// Today's date as YYYY-MM-DD in the *local* timezone. `toISOString().slice(0, 10)` gives the UTC date
// instead, which is yesterday for anyone east of UTC early in their morning.
export function todayIsoDate(now: Date = new Date()): string {
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${now.getFullYear()}-${month}-${day}`
}
