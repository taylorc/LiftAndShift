// Mirrors LiftAndShift.Core.Workouts.WorkoutLifts.For server-side: which lifts a workout means,
// given the family member's current Training Phase. Workout A never changes; Workout B's third lift
// depends on phase (see CONTEXT.md > Training Phase).
const PHASE_THIRD_LIFT: Record<number, string> = {
  1: 'Deadlift',
  2: 'Row',
  3: 'LatPulldown'
}

export function liftsForWorkout(workout: 'A' | 'B', trainingPhase: number): string[] {
  if (workout === 'A') {
    return ['Squat', 'Press', 'Deadlift']
  }

  return ['Squat', 'BenchPress', PHASE_THIRD_LIFT[trainingPhase]]
}
