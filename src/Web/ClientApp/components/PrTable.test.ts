import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import PrTable from './PrTable.vue'
import type { PersonalRecordSummaryDto } from '~/lib/web-api-client'

describe('PrTable', () => {
  it('shows a placeholder message when there are no records', async () => {
    const wrapper = await mountSuspended(PrTable, { props: { records: [] } })

    expect(wrapper.text()).toContain('No personal records yet')
    expect(wrapper.find('table').exists()).toBe(false)
  })

  it('renders a row per record with the estimated 1RM rounded to one decimal', async () => {
    const records: PersonalRecordSummaryDto[] = [
      {
        exerciseId: 1,
        exerciseName: 'Squat',
        weightKg: 100,
        reps: 5,
        estimated1RmKg: 116.666,
        achievedAt: '2026-01-01T00:00:00Z',
      } as PersonalRecordSummaryDto,
    ]

    const wrapper = await mountSuspended(PrTable, { props: { records } })

    expect(wrapper.find('table').exists()).toBe(true)
    const row = wrapper.find('tbody tr')
    expect(row.text()).toContain('Squat')
    expect(row.text()).toContain('116.7')
  })
})
