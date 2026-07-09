<script setup lang="ts">
const auth = useAuthStore()
const router = useRouter()

async function handleLogout() {
  await auth.logout()
  router.push('/login')
}
</script>

<template>
  <template v-if="auth.isAuthenticated">
    <li><a href="#" @click.prevent="handleLogout">Log out</a></li>
    <li><NuxtLink to="/usermanagement">Account</NuxtLink></li>
  </template>
  <template v-else>
    <li><NuxtLink to="/login">Log in</NuxtLink></li>
    <li><NuxtLink to="/register">Register</NuxtLink></li>
  </template>
</template>
