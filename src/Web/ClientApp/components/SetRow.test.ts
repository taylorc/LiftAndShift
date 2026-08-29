import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import SetRow from './SetRow.vue'
import type { LogWorkoutSetDto } from '~/lib/web-api-client'

function baseSet(overrides: Partial<LogWorkoutSetDto> = {}): LogWorkoutSetDto {
  return {
    setNumber: 1,
    setType: 1,
    weightKg: 100,
    reps: 5,
    completedReps: null,
    notes: null,
    isCompleted: false,
    ...overrides,
  } as LogWorkoutSetDto
}

describe('SetRow', () => {
  it('emits update with the parsed weight when the weight input changes', async () => {
    const wrapper = await mountSuspended(SetRow, { props: { set: baseSet() } })

    const weightInput = wrapper.findAll('input[type="number"]')[0]
    await weightInput.setValue('102.5')
    await weightInput.trigger('change')

    expect(wrapper.emitted('update')?.[0]).toEqual(['weightKg', 102.5])
  })

  it('emits update with null completedReps when the field is cleared', async () => {
    const wrapper = await mountSuspended(SetRow, { props: { set: baseSet({ completedReps: 5 }) } })

    const completedRepsInput = wrapper.findAll('input[type="number"]')[2]
    await completedRepsInput.setValue('')
    await completedRepsInput.trigger('change')

    expect(wrapper.emitted('update')?.[0]).toEqual(['completedReps', null])
  })

  it('emits update with the checked state when Done is toggled', async () => {
    const wrapper = await mountSuspended(SetRow, { props: { set: baseSet() } })

    const checkbox = wrapper.find('input[type="checkbox"]')
    await checkbox.setValue(true)

    expect(wrapper.emitted('update')?.[0]).toEqual(['isCompleted', true])
  })

  it('emits remove when the remove button is clicked', async () => {
    const wrapper = await mountSuspended(SetRow, { props: { set: baseSet() } })

    await wrapper.get('button').trigger('click')

    expect(wrapper.emitted('remove')).toBeTruthy()
  })
})
