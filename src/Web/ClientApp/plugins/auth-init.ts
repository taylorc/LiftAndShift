export default defineNuxtPlugin(async () => {
  const auth = useAuthStore()
  // Composables must be called from plugin scope, not inside a Pinia action,
  // because the Nuxt request context is not propagated into store actions during SSR.
  try {
    const usersClient = useUsersClient()
    await usersClient.infoGET()
    auth.isAuthenticated = true
    const onboardingClient = useOnboardingClient()
    const data = await onboardingClient.getOnboarding()
    auth.isOnboarded = data.isOnboarded ?? false
  } catch {
    auth.isAuthenticated = false
    auth.isOnboarded = false
  } finally {
    auth.isLoading = false
  }
})
