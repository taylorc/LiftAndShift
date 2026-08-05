import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import NavMenu from './NavMenu.vue'
import { useAuthStore } from '~/stores/auth'
import { useProgrammeStore } from '~/stores/programme'

describe('NavMenu', () => {
  it('only shows login/register links when logged out', async () => {
    const wrapper = await mountSuspended(NavMenu)

    expect(wrapper.text()).toContain('Log in')
    expect(wrapper.text()).toContain('Register')
    expect(wrapper.text()).not.toContain('Dashboard')
  })

  it('hides programme-scoped links until a programme is active', async () => {
    const wrapper = await mountSuspended(NavMenu)
    useAuthStore().isAuthenticated = true
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Programme')
    expect(wrapper.text()).toContain('Settings')
    expect(wrapper.text()).not.toContain('Dashboard')
    expect(wrapper.text()).not.toContain('Log Workout')
  })

  it('shows the full menu once a programme is active', async () => {
    const wrapper = await mountSuspended(NavMenu)
    useAuthStore().isAuthenticated = true
    useProgrammeStore().activeProgramme = { id: 1 } as any
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Dashboard')
    expect(wrapper.text()).toContain('Log Workout')
  })
})
