// @vitest-environment nuxt
import { describe, expect, it } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import WorkoutPicker from './WorkoutPicker.vue'

const workoutLifts = {
  A: ['Squat', 'Press', 'Deadlift'],
  B: ['Squat', 'BenchPress', 'Row']
}

describe('WorkoutPicker', () => {
  it('lists the lifts the server resolved for each workout', async () => {
    const wrapper = await mountSuspended(WorkoutPicker, { props: { workoutLifts } })

    const buttons = wrapper.findAll('button')
    expect(buttons[0]!.text()).toContain('Squat · Press · Deadlift')
    expect(buttons[1]!.text()).toContain('Squat · Bench Press · Row')
  })

  it('emits the chosen workout', async () => {
    const wrapper = await mountSuspended(WorkoutPicker, { props: { workoutLifts } })

    await wrapper.findAll('button')[1]!.trigger('click')

    expect(wrapper.emitted('choose')).toEqual([['B']])
  })
})
