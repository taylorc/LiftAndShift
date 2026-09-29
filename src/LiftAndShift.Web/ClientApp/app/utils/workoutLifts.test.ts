import { describe, expect, it } from 'vitest'
import { liftsForWorkout } from './workoutLifts'

describe('liftsForWorkout', () => {
  it('workout A is always the same three lifts regardless of Training Phase', () => {
    expect(liftsForWorkout('A', 1)).toEqual(['Squat', 'Press', 'Deadlift'])
    expect(liftsForWorkout('A', 3)).toEqual(['Squat', 'Press', 'Deadlift'])
  })

  it("workout B's third lift depends on Training Phase", () => {
    expect(liftsForWorkout('B', 1)).toEqual(['Squat', 'BenchPress', 'Deadlift'])
    expect(liftsForWorkout('B', 2)).toEqual(['Squat', 'BenchPress', 'Row'])
    expect(liftsForWorkout('B', 3)).toEqual(['Squat', 'BenchPress', 'LatPulldown'])
  })
})
