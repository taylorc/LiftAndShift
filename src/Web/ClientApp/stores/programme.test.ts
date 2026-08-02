import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'
import { setActivePinia, createPinia } from 'pinia'
import { useProgrammeStore } from './programme'
import type { LogProgrammeSessionCommand } from '~/lib/web-api-client'

const { getProgrammeTemplates, adoptProgramme, getActiveProgramme, logProgrammeSession } = vi.hoisted(() => ({
  getProgrammeTemplates: vi.fn(),
  adoptProgramme: vi.fn(),
  getActiveProgramme: vi.fn(),
  logProgrammeSession: vi.fn()
}))

mockNuxtImport('useProgrammesClient', () => {
  return () => ({ getProgrammeTemplates, adoptProgramme, getActiveProgramme, logProgrammeSession })
})


describe('useProgrammeStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('has no active programme by default', () => {
    const store = useProgrammeStore()
    expect(store.hasActiveProgramme).toBe(false)
  })

  it('reports an active programme once one is set', () => {
    const store = useProgrammeStore()
    store.activeProgramme = { id: 1 } as any
    expect(store.hasActiveProgramme).toBe(true)
  })

  it('will set templates property when fetchTemplates is called', async () => {
    const store = useProgrammeStore()
    getProgrammeTemplates.mockResolvedValueOnce([{ id: 2 }])
    await store.fetchTemplates()
    expect(store.templates.length).toBeGreaterThan(0)
    expect(store.templates[0].id).toBe(2)
  })

  it('will call the api function adoptProgramme with the correct parameters when adoptProgramme is called', async () => {
    const store = useProgrammeStore()
    adoptProgramme.mockResolvedValueOnce(5)
    getActiveProgramme.mockResolvedValueOnce({ id: 1 })

    const command = { templateId: 1 } as any
    await store.adoptProgramme(command)

    expect(adoptProgramme).toHaveBeenCalledTimes(1)
    expect(adoptProgramme).toHaveBeenCalledWith(command)
  })

  it('will call the api function logProgrammeSession with the correct parameters when logProgrammeSession is called', async () => {
    const store = useProgrammeStore()
    logProgrammeSession.mockResolvedValueOnce(5)
    getActiveProgramme.mockResolvedValueOnce({ id: 1 })

    const command = { userProgrammeId:5, programmeSessionId: 4,   } as LogProgrammeSessionCommand
    await store.logProgrammeSession(5, command)

    expect(logProgrammeSession).toHaveBeenCalledTimes(1)
    expect(logProgrammeSession).toHaveBeenCalledWith(5, command)
  })

  it('sets activeProgramme from the mocked client', async () => {
    getActiveProgramme.mockResolvedValueOnce({ id: 42 })

    const { useProgrammeStore } = await import('./programme')
    setActivePinia(createPinia())
    const store = useProgrammeStore()

    await store.fetchActiveProgramme()

    expect(store.activeProgramme).toEqual({ id: 42 })
    expect(store.hasActiveProgramme).toBe(true)
  })

  it('normalizes an empty response to null', async () => {
    getActiveProgramme.mockResolvedValueOnce({})

    const { useProgrammeStore } = await import('./programme')
    setActivePinia(createPinia())
    const store = useProgrammeStore()

    await store.fetchActiveProgramme()

    expect(store.activeProgramme).toBeNull()
  })
})
