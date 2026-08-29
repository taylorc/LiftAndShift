import { describe, it, expect, vi, afterEach } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import RestTimer from './RestTimer.vue'

describe('RestTimer', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('defaults to 3:00 and counts down each second', async () => {
    vi.useFakeTimers()
    const wrapper = await mountSuspended(RestTimer)

    expect(wrapper.text()).toContain('3:00')

    await vi.advanceTimersByTimeAsync(1000)
    expect(wrapper.text()).toContain('2:59')
  })

  it('formats a custom seconds prop', async () => {
    vi.useFakeTimers()
    const wrapper = await mountSuspended(RestTimer, { props: { seconds: 65 } })

    expect(wrapper.text()).toContain('1:05')
  })

  it('emits done when it reaches zero', async () => {
    vi.useFakeTimers()
    const wrapper = await mountSuspended(RestTimer, { props: { seconds: 2 } })

    await vi.advanceTimersByTimeAsync(3000)

    expect(wrapper.emitted('done')).toBeTruthy()
  })

  it('resets the remaining time when Reset is clicked', async () => {
    vi.useFakeTimers()
    const wrapper = await mountSuspended(RestTimer, { props: { seconds: 10 } })

    await vi.advanceTimersByTimeAsync(5000)
    expect(wrapper.text()).toContain('0:05')

    const resetButton = wrapper.findAll('button').find((b) => b.text() === 'Reset')!
    await resetButton.trigger('click')

    expect(wrapper.text()).toContain('0:10')
  })

  it('emits done when Done is clicked', async () => {
    vi.useFakeTimers()
    const wrapper = await mountSuspended(RestTimer)

    const doneButton = wrapper.findAll('button').find((b) => b.text() === 'Done')!
    await doneButton.trigger('click')

    expect(wrapper.emitted('done')).toBeTruthy()
  })
})
