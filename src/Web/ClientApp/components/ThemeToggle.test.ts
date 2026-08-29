import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import ThemeToggle from './ThemeToggle.vue'
import { useThemeStore } from '~/stores/theme'

describe('ThemeToggle', () => {
  it('exposes the current theme as its aria-label', async () => {
    const wrapper = await mountSuspended(ThemeToggle)
    useThemeStore().theme = 'dark'
    await wrapper.vm.$nextTick()

    expect(wrapper.get('button').attributes('aria-label')).toBe('dark')
  })

  it('cycles the theme when clicked', async () => {
    const wrapper = await mountSuspended(ThemeToggle)
    const theme = useThemeStore()
    theme.theme = 'dark'
    await wrapper.vm.$nextTick()

    await wrapper.get('button').trigger('click')

    expect(theme.theme).toBe('auto')
  })
})
