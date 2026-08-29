import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '~/stores/auth'

const { navigateTo, useRoute } = vi.hoisted(() => ({
  navigateTo: vi.fn((to: any) => ({ __redirect: to })),
  useRoute: vi.fn(() => ({ fullPath: '/dashboard' })),
}))

mockNuxtImport('navigateTo', () => navigateTo)
mockNuxtImport('useRoute', () => useRoute)

describe('protected middleware', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    navigateTo.mockImplementation((to: any) => ({ __redirect: to }))
    useRoute.mockReturnValue({ fullPath: '/dashboard' })
  })

  it('redirects to login with a returnUrl when not authenticated', async () => {
    const middleware = (await import('./protected')).default
    const auth = useAuthStore()
    auth.isAuthenticated = false
    auth.isOnboarded = true

    const result = middleware({} as any, {} as any)

    expect(navigateTo).toHaveBeenCalledWith({ path: '/login', query: { returnUrl: '/dashboard' } })
    expect(result).toEqual({ __redirect: { path: '/login', query: { returnUrl: '/dashboard' } } })
  })

  it('redirects to onboarding when authenticated but not onboarded', async () => {
    const middleware = (await import('./protected')).default
    const auth = useAuthStore()
    auth.isAuthenticated = true
    auth.isOnboarded = false

    const result = middleware({} as any, {} as any)

    expect(navigateTo).toHaveBeenCalledWith('/onboarding')
    expect(result).toEqual({ __redirect: '/onboarding' })
  })

  it('allows navigation through when authenticated and onboarded', async () => {
    const middleware = (await import('./protected')).default
    const auth = useAuthStore()
    auth.isAuthenticated = true
    auth.isOnboarded = true

    const result = middleware({} as any, {} as any)

    expect(navigateTo).not.toHaveBeenCalled()
    expect(result).toBeUndefined()
  })
})
