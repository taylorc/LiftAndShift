export default defineNuxtPlugin(async () => {
  const auth = useAuthStore()
  // Composables must be called from plugin scope, not inside a Pinia action,
  // because the Nuxt request context is not propagated into store actions during SSR.
  try {
    const usersClient = useUsersClient()
    await usersClient.infoGET()
    auth.isAuthenticated = true
  } catch {
    auth.isAuthenticated = false
  } finally {
    auth.isLoading = false
  }
})
