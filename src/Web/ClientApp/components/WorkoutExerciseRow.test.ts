import { describe, it, expect, vi, afterEach } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import WorkoutExerciseRow from './WorkoutExerciseRow.vue'
import type { LogWorkoutExerciseDto, LogWorkoutSetDto } from '~/lib/web-api-client'

function exerciseWith(sets: LogWorkoutSetDto[], exerciseName?: string): LogWorkoutExerciseDto & { exerciseName?: string } {
  return {
    exerciseId: 1,
    orderIndex: 0,
    notes: null,
    sets,
    exerciseName,
  } as any
}

function set(overrides: Partial<LogWorkoutSetDto> = {}): LogWorkoutSetDto {
  return {
    setNumber: 1,
    setType: 1,
    weightKg: 100,
    reps: 5,
    completedReps: null,
    notes: null,
    isCompleted: false,
    ...overrides,
  } as LogWorkoutSetDto
}

describe('WorkoutExerciseRow', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('falls back to a positional name when the exercise has none', async () => {
    const wrapper = await mountSuspended(WorkoutExerciseRow, {
      props: { exercise: exerciseWith([set()]), index: 2 },
    })

    expect(wrapper.text()).toContain('Exercise 3')
  })

  it('uses the exercise name when provided', async () => {
    const wrapper = await mountSuspended(WorkoutExerciseRow, {
      props: { exercise: exerciseWith([set()], 'Squat'), index: 0 },
    })

    expect(wrapper.text()).toContain('Squat')
  })

  it('emits update with a new set appended, carrying forward the last set weight/reps', async () => {
    const wrapper = await mountSuspended(WorkoutExerciseRow, {
      props: { exercise: exerciseWith([set({ weightKg: 100, reps: 5 })]), index: 0 },
    })

    await wrapper.get('button.outline:not(.secondary)').trigger('click')

    const updated = wrapper.emitted('update')?.[0][0] as LogWorkoutExerciseDto
    expect(updated.sets).toHaveLength(2)
    expect(updated.sets[1]).toMatchObject({ setNumber: 2, weightKg: 100, reps: 5, isCompleted: false })
  })

  it('emits update with the set removed and remaining sets renumbered', async () => {
    const wrapper = await mountSuspended(WorkoutExerciseRow, {
      props: { exercise: exerciseWith([set({ setNumber: 1 }), set({ setNumber: 2 })]), index: 0 },
    })

    const removeButtons = wrapper.findAll('tbody button')
    await removeButtons[0].trigger('click')

    const updated = wrapper.emitted('update')?.[0][0] as LogWorkoutExerciseDto
    expect(updated.sets).toHaveLength(1)
    expect(updated.sets[0].setNumber).toBe(1)
  })

  it('emits remove when the exercise Remove button is clicked', async () => {
    const wrapper = await mountSuspended(WorkoutExerciseRow, {
      props: { exercise: exerciseWith([set()]), index: 0 },
    })

    await wrapper.get('header button').trigger('click')

    expect(wrapper.emitted('remove')).toBeTruthy()
  })

  it('shows the rest timer once a set is marked done', async () => {
    vi.useFakeTimers()
    const wrapper = await mountSuspended(WorkoutExerciseRow, {
      props: { exercise: exerciseWith([set()]), index: 0 },
    })

    expect(wrapper.findComponent({ name: 'RestTimer' }).exists()).toBe(false)

    const checkbox = wrapper.find('input[type="checkbox"]')
    await checkbox.setValue(true)

    expect(wrapper.text()).toContain('Rest:')
  })
})
