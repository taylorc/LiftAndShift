import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import ProgressChart from './ProgressChart.vue'

describe('ProgressChart', () => {
  it('renders nothing when there is no data', async () => {
    const wrapper = await mountSuspended(ProgressChart, { props: { data: [], yField: 'weightKg' } })

    expect(wrapper.findAll('circle')).toHaveLength(0)
    expect(wrapper.find('polyline').exists()).toBe(false)
  })

  it('renders a point per data entry but no connecting line for a single point', async () => {
    const wrapper = await mountSuspended(ProgressChart, {
      props: { data: [{ date: '2026-01-01', weightKg: 100 }], yField: 'weightKg' },
    })

    expect(wrapper.findAll('circle')).toHaveLength(1)
    expect(wrapper.find('polyline').exists()).toBe(false)
  })

  it('renders a connecting line and one point per entry for multiple points', async () => {
    const wrapper = await mountSuspended(ProgressChart, {
      props: {
        data: [
          { date: '2026-01-01', weightKg: 100 },
          { date: '2026-01-08', weightKg: 105 },
          { date: '2026-01-15', weightKg: 110 },
        ],
        yField: 'weightKg',
      },
    })

    expect(wrapper.findAll('circle')).toHaveLength(3)
    expect(wrapper.find('polyline').exists()).toBe(true)
  })

  it('uses the given width and height for the viewBox', async () => {
    const wrapper = await mountSuspended(ProgressChart, {
      props: {
        data: [{ date: '2026-01-01', weightKg: 100 }],
        yField: 'weightKg',
        width: 400,
        height: 200,
      },
    })

    expect(wrapper.find('svg').attributes('viewBox')).toBe('0 0 400 200')
  })
})
