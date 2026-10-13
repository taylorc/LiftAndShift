// @vitest-environment nuxt
import { describe, expect, it } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import type { Lift } from '~/types'
import WorkoutEntry from './WorkoutEntry.vue'

const lift = (id: number, name: string, workSetCount = 3): Lift => ({
  id,
  name,
  openingWeightKg: 20,
  incrementKg: 5,
  workSetCount
})

const liftsInWorkout = [lift(1, 'Squat'), lift(2, 'Press'), lift(4, 'Deadlift', 1)]

const baseProps = {
  workout: 'A' as const,
  liftsInWorkout,
  loadingWorkWeights: false,
  submitting: false,
  submitError: null
}

describe('WorkoutEntry', () => {
  it('blocks logging and names every lift not yet Ramped', async () => {
    const wrapper = await mountSuspended(WorkoutEntry, {
      props: { ...baseProps, workWeights: { Squat: 40, Press: null, Deadlift: null } }
    })

    expect(wrapper.text()).toContain("You haven't Ramped Press, Deadlift yet")
    expect(wrapper.find('form').exists()).toBe(false)
  })

  it('shows the form once every lift has a Work Weight', async () => {
    const wrapper = await mountSuspended(WorkoutEntry, {
      props: { ...baseProps, workWeights: { Squat: 40, Press: 30, Deadlift: 90 } }
    })

    expect(wrapper.find('form').exists()).toBe(true)
    expect(wrapper.text()).not.toContain("You haven't Ramped")
  })

  it('shows only the loading message while Work Weights load', async () => {
    const wrapper = await mountSuspended(WorkoutEntry, {
      props: { ...baseProps, loadingWorkWeights: true, workWeights: {} }
    })

    expect(wrapper.text()).toContain('Loading your Work Weights')
    expect(wrapper.find('form').exists()).toBe(false)
  })

  it('emits change when the workout is changed', async () => {
    const wrapper = await mountSuspended(WorkoutEntry, {
      props: { ...baseProps, workWeights: { Squat: 40, Press: 30, Deadlift: 90 } }
    })

    await wrapper.find('button[type=button]').trigger('click')

    expect(wrapper.emitted('change')).toHaveLength(1)
  })
})
