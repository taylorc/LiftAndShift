<script setup lang="ts">
const auth = useAuthStore()
const programme = useProgrammeStore()
const router = useRouter()

async function handleLogout() {
  await auth.logout()
  router.push('/login')
}

watch(
  () => auth.isAuthenticated,
  (isAuthenticated) => {
    if (isAuthenticated) programme.fetchActiveProgramme()
  },
  { immediate: true },
)
</script>

<template>
  <template v-if="auth.isAuthenticated">
    <template v-if="programme.hasActiveProgramme">
      <li><NuxtLink to="/dashboard">Dashboard</NuxtLink></li>
      <li><NuxtLink to="/log-workout">Log Workout</NuxtLink></li>
      <li><NuxtLink to="/history">History</NuxtLink></li>
      <li><NuxtLink to="/progress">Progress</NuxtLink></li>
      <li><NuxtLink to="/calculators">Calculators</NuxtLink></li>
      <li><NuxtLink to="/exercise-library">Exercises</NuxtLink></li>
    </template>
    <li><NuxtLink to="/programme">Programme</NuxtLink></li>
    <li><NuxtLink to="/settings">Settings</NuxtLink></li>
    <li><a href="#" @click.prevent="handleLogout">Log out</a></li>
    <li><NuxtLink to="/usermanagement">Account</NuxtLink></li>
  </template>
  <template v-else>
    <li><NuxtLink to="/login">Log in</NuxtLink></li>
    <li><NuxtLink to="/register">Register</NuxtLink></li>
  </template>
</template>
