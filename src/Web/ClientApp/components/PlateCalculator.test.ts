import { describe, it, expect, vi } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import { flushPromises } from '@vue/test-utils'
import PlateCalculator from './PlateCalculator.vue'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'
import { setActivePinia, createPinia } from 'pinia'

const { getPlateCalculation } = vi.hoisted(() => ({
  getPlateCalculation: vi.fn(),
}))

mockNuxtImport('useCalculatorsClient', () => {
  return () => ({ getPlateCalculation }) as any
})

describe('PlateCalculator', () => {
  it('returns the correct plate size array for the visual', async () => {

    /* isExact?: boolean;
    actualWeightKg?: number;
    platesPerSide?: { [key: string]: number; };*/
    getPlateCalculation.mockResolvedValueOnce({ isExact: true, actualWeightKg: 200, platesPerSide: { '25': 2 } })

    const wrapper = await mountSuspended(PlateCalculator)
    const label = wrapper.findAll('label').find(l => l.text().includes('Target weight'))
    const targetInput = label!.find('input')
    await targetInput.setValue("200")

    await wrapper.find('form').trigger('submit')
    await flushPromises()

    const leftPlates = wrapper.find('[id="leftPlates"]')
    const rightPlates = wrapper.find('[id="rightPlates"]')

    expect(leftPlates.html()).toContain('25')
    expect(rightPlates.html()).toContain('25')
    
  })

  // it('hides programme-scoped links until a programme is active', async () => {
  //   const wrapper = await mountSuspended(PlateCalculator)
  //   useAuthStore().isAuthenticated = true
  //   await wrapper.vm.$nextTick()

  //   expect(wrapper.text()).toContain('Programme')
  //   expect(wrapper.text()).toContain('Settings')
  //   expect(wrapper.text()).not.toContain('Dashboard')
  //   expect(wrapper.text()).not.toContain('Log Workout')
  // })

  // it('shows the full menu once a programme is active', async () => {
  //   const wrapper = await mountSuspended(PlateCalculator)
  //   useAuthStore().isAuthenticated = true
  //   useProgrammeStore().activeProgramme = { id: 1 } as any
  //   await wrapper.vm.$nextTick()

  //   expect(wrapper.text()).toContain('Dashboard')
  //   expect(wrapper.text()).toContain('Log Workout')
  // })
})