import { defineStore } from 'pinia'
import { LoginRequest, RegisterRequest } from '~/lib/web-api-client'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    isAuthenticated: false,
    isOnboarded: false,
    isLoading: true,
  }),

  actions: {
    async initAuth() {
      const usersClient = useUsersClient()
      const onboardingClient = useOnboardingClient()
      try {
        await usersClient.infoGET()
        this.isAuthenticated = true
        const data = await onboardingClient.getOnboarding()
        this.isOnboarded = data.isOnboarded ?? false
      } catch {
        this.isAuthenticated = false
        this.isOnboarded = false
      } finally {
        this.isLoading = false
      }
    },

    async fetchOnboardingStatus() {
      const client = useOnboardingClient()
      try {
        const data = await client.getOnboarding()
        this.isOnboarded = data.isOnboarded ?? false
      } catch {
        this.isOnboarded = false
      }
    },

    async login(email: string, password: string) {
      const client = useUsersClient()
      await client.login(true, undefined, new LoginRequest({ email, password }))
      this.isAuthenticated = true
      await this.fetchOnboardingStatus()
    },

    async register(email: string, password: string) {
      const client = useUsersClient()
      await client.register(new RegisterRequest({ email, password }))
    },

    async logout() {
      const client = useUsersClient()
      await client.logout({})
      this.isAuthenticated = false
      this.isOnboarded = false
    },
  },
})
