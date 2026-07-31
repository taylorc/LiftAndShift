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
      try {
        await usersClient.infoGET()
        this.isAuthenticated = true
      } catch {
        this.isAuthenticated = false
        this.isOnboarded = false
      } finally {
        this.isLoading = false
      }
    },

    async login(email: string, password: string) {
      const client = useUsersClient()
      await client.login(true, undefined, new LoginRequest({ email, password }))
      this.isAuthenticated = true
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
