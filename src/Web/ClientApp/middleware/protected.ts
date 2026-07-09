export default defineNuxtRouteMiddleware(() => {
  const auth = useAuthStore()
  if (!auth.isAuthenticated) {
    return navigateTo({ path: '/login', query: { returnUrl: useRoute().fullPath } })
  }
  if (!auth.isOnboarded) {
    return navigateTo('/onboarding')
  }
})
