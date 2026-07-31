import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'
import { setActivePinia, createPinia } from 'pinia'
import { useProgrammeStore } from './programme'

const { getProgrammeTemplates } = vi.hoisted(() => ({
  getProgrammeTemplates: vi.fn(),
}))

mockNuxtImport('useProgrammesClient', () => {
  return () => ({ getProgrammeTemplates }) as any
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
})
