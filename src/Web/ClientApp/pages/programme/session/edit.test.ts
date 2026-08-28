import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime'
import { flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import EditSession from './[sessionId]/edit.vue'

const { getActiveProgramme, getProgrammeTemplates, getProgrammeSessions, editProgrammeSession } = vi.hoisted(() => ({
  getActiveProgramme: vi.fn(),
  getProgrammeTemplates: vi.fn(),
  getProgrammeSessions: vi.fn(),
  editProgrammeSession: vi.fn(),
}))

mockNuxtImport('useProgrammesClient', () => () => ({
  getActiveProgramme,
  getProgrammeTemplates,
  getProgrammeSessions,
  editProgrammeSession,
}))
mockNuxtImport('useRoute', () => () => ({ params: { sessionId: '11' } }))

const session11 = {
  sessionId: 11,
  workoutSessionId: 101,
  workoutType: 'A',
  completedDate: '2026-01-01T00:00:00Z',
  exercises: [
    {
      exerciseId: 1,
      exerciseName: 'Squat',
      orderIndex: 0,
      sets: [
        { setNumber: 1, setType: 'Warmup', weightKg: 20, reps: 5, completedReps: 5, isCompleted: true },
        { setNumber: 2, setType: 'WorkingSet', weightKg: 60, reps: 5, completedReps: 5, isCompleted: true },
      ],
    },
  ],
}

describe('edit logged session page', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    getActiveProgramme.mockResolvedValue({ id: 7 })
    getProgrammeTemplates.mockResolvedValue([])
    getProgrammeSessions.mockResolvedValue([session11])
    editProgrammeSession.mockResolvedValue(undefined)
  })

  it('prefills the set grid from the logged session', async () => {
    const wrapper = await mountSuspended(EditSession)
    await flushPromises()

    expect(wrapper.text()).toContain('Squat')
    const rows = wrapper.findAll('tbody tr')
    expect(rows.length).toBe(2)
    const weightInputs = wrapper.findAll('tbody tr td:nth-child(3) input')
    expect((weightInputs[1].element as HTMLInputElement).value).toBe('60')
  })

  it('disables Save while a working set is missing completed reps', async () => {
    getProgrammeSessions.mockResolvedValue([
      {
        ...session11,
        exercises: [
          {
            exerciseId: 1,
            exerciseName: 'Squat',
            orderIndex: 0,
            sets: [
              { setNumber: 1, setType: 'WorkingSet', weightKg: 60, reps: 5, completedReps: null, isCompleted: false },
            ],
          },
        ],
      },
    ])

    const wrapper = await mountSuspended(EditSession)
    await flushPromises()

    const save = wrapper.findAll('button').find((b) => b.text().toLowerCase().includes('save'))!
    expect((save.element as HTMLButtonElement).disabled).toBe(true)
    expect(wrapper.text().toLowerCase()).toContain('completed reps')
  })

  it('saves edited set data via editProgrammeSession and returns to the programme page', async () => {
    const wrapper = await mountSuspended(EditSession)
    await flushPromises()

    const push = vi.spyOn(useRouter(), 'push')

    const workingWeight = wrapper.findAll('tbody tr td:nth-child(3) input')[1]
    await workingWeight.setValue('62.5')

    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(editProgrammeSession).toHaveBeenCalledTimes(1)
    const [programmeId, sessionId, command] = editProgrammeSession.mock.calls[0]
    expect(programmeId).toBe(7)
    expect(sessionId).toBe(11)
    expect(command.exercises[0].sets[1].weightKg).toBe(62.5)
    expect(command.exercises[0].sets[1].setType).toBe(1) // WorkingSet -> enum value
    expect(command.exercises[0].sets[0].setType).toBe(0) // Warmup -> enum value
    expect(push).toHaveBeenCalledWith('/programme')
  })
})
