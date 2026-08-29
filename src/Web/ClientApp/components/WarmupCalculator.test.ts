import { describe, it, expect, vi } from 'vitest'
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime'
import { flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import WarmupCalculator from './WarmupCalculator.vue'
import { useCalculatorStore } from '~/stores/calculator'

const { getWarmupSets } = vi.hoisted(() => ({
  getWarmupSets: vi.fn(),
}))

mockNuxtImport('useCalculatorsClient', () => () => ({ getWarmupSets }))

describe('WarmupCalculator', () => {
  it('shows no table until a calculation has been made', async () => {
    setActivePinia(createPinia())
    const wrapper = await mountSuspended(WarmupCalculator)
    useCalculatorStore().warmupSets = []

    expect(wrapper.find('table').exists()).toBe(false)
  })

  it('calls the client with the form values and renders the returned warmup sets', async () => {
    setActivePinia(createPinia())
    getWarmupSets.mockResolvedValueOnce([
      { setNumber: 1, weightKg: 20, reps: 5 },
      { setNumber: 2, weightKg: 60, reps: 3 },
    ])

    const wrapper = await mountSuspended(WarmupCalculator)

    const weightLabel = wrapper.findAll('label').find((l) => l.text().includes('Working weight'))!
    await weightLabel.find('input').setValue('120')

    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(getWarmupSets).toHaveBeenCalledWith(120, 20, 4)

    const rows = wrapper.findAll('tbody tr')
    expect(rows).toHaveLength(3) // 2 warmup sets + working weight row
    expect(rows[0].text()).toContain('20')
    expect(rows[2].text()).toContain('Working')
    expect(rows[2].text()).toContain('120')
  })
})
