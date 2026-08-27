import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime'
import { flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import LogSession from './log-session.vue'

const { getActiveProgramme, getExercises } = vi.hoisted(() => ({
  getActiveProgramme: vi.fn(),
  getExercises: vi.fn(),
}))

mockNuxtImport('useProgrammesClient', () => () => ({ getActiveProgramme }))
mockNuxtImport('useExercisesClient', () => () => ({ getExercises }))

const nextSession = {
  sessionId: 1,
  workoutType: 'A',
  prescribedLifts: [
    {
      liftName: 'Squat',
      weightKg: 100,
      sets: 3,
      reps: 5,
      warmupSets: [
        { setNumber: 1, weightKg: 20, reps: 5 },
        { setNumber: 2, weightKg: 60, reps: 5 },
      ],
    },
  ],
}

describe('log-session page', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    getActiveProgramme.mockResolvedValue({ id: 7, nextSession })
    getExercises.mockResolvedValue([{ id: 99, name: 'Squat' }])
  })

  it('defaults every Done checkbox to unchecked when prefilling from the programme', async () => {
    const wrapper = await mountSuspended(LogSession)
    await flushPromises()

    const doneCheckboxes = wrapper.findAll('tbody input[type="checkbox"]')
    expect(doneCheckboxes.length).toBe(5) // 2 warmup + 3 working sets

    for (const cb of doneCheckboxes) {
      expect((cb.element as HTMLInputElement).checked).toBe(false)
    }
  })
})
