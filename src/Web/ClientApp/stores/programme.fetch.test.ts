import { describe, it, expect, vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'
import { setActivePinia, createPinia } from 'pinia'

const { getActiveProgramme } = vi.hoisted(() => ({
  getActiveProgramme: vi.fn(),
}))

mockNuxtImport('useProgrammesClient', () => {
  return () => ({ getActiveProgramme }) as any
})

describe('useProgrammeStore fetchActiveProgramme', () => {
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
