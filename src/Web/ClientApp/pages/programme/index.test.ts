import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime'
import { flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import Programme from './index.vue'

const {
  getActiveProgramme,
  getProgrammeTemplates,
  getProgrammeSessions,
  deleteProgrammeSession,
  updateProgramme,
} = vi.hoisted(() => ({
  getActiveProgramme: vi.fn(),
  getProgrammeTemplates: vi.fn(),
  getProgrammeSessions: vi.fn(),
  deleteProgrammeSession: vi.fn(),
  updateProgramme: vi.fn(),
}))

mockNuxtImport('useProgrammesClient', () => () => ({
  getActiveProgramme,
  getProgrammeTemplates,
  getProgrammeSessions,
  deleteProgrammeSession,
  updateProgramme,
}))

const loggedSessions = [
  {
    sessionId: 11,
    workoutSessionId: 101,
    workoutType: 'A',
    completedDate: '2026-01-01T00:00:00Z',
    exercises: [{ exerciseId: 1, exerciseName: 'Squat', orderIndex: 0, sets: [] }],
  },
  {
    sessionId: 12,
    workoutSessionId: 102,
    workoutType: 'B',
    completedDate: '2026-01-03T00:00:00Z',
    exercises: [{ exerciseId: 1, exerciseName: 'Squat', orderIndex: 0, sets: [] }],
  },
]

describe('programme page — past sessions', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    getActiveProgramme.mockResolvedValue({ id: 7, programmeName: 'Starting Strength', sessionCount: 2, startedAt: '2026-01-01T00:00:00Z' })
    getProgrammeTemplates.mockResolvedValue([])
    getProgrammeSessions.mockResolvedValue(loggedSessions)
  })

  it('lists the logged sessions with a link to each edit page', async () => {
    const wrapper = await mountSuspended(Programme)
    await flushPromises()

    expect(wrapper.text()).toContain('Past sessions')
    const editLinks = wrapper.findAll('a[href="/programme/session/11/edit"]')
    expect(editLinks.length).toBe(1)
    expect(wrapper.find('a[href="/programme/session/12/edit"]').exists()).toBe(true)
  })

  it('offers a delete button only for the most recent logged session', async () => {
    const wrapper = await mountSuspended(Programme)
    await flushPromises()

    const deleteButtons = wrapper.findAll('button').filter((b) => b.text().toLowerCase().includes('delete'))
    expect(deleteButtons.length).toBe(1)

    await deleteButtons[0].trigger('click')
    expect(deleteProgrammeSession).toHaveBeenCalledWith(7, 12)
  })

  it('saves programme metadata changes via updateProgramme', async () => {
    const wrapper = await mountSuspended(Programme)
    await flushPromises()

    const form = wrapper.find('form.programme-meta')
    expect(form.exists()).toBe(true)

    await form.find('select').setValue('1') // Paused
    await form.trigger('submit')
    await flushPromises()

    expect(updateProgramme).toHaveBeenCalledTimes(1)
    const [programmeId, command] = updateProgramme.mock.calls[0]
    expect(programmeId).toBe(7)
    expect(command.status).toBe(1)
  })
})
