<script setup lang="ts">
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const email = ref('')
const password = ref('')
const invalid = ref(false)

async function handleSubmit() {
  try {
    await auth.login(email.value, password.value)
    if (!auth.isOnboarded) {
      router.replace('/onboarding')
    } else {
      router.replace((route.query.returnUrl as string) || '/')
    }
  } catch {
    invalid.value = true
  }
}

function clearInvalid() {
  invalid.value = false
}
</script>

<template>
  <article>
    <h2>Log in</h2>
    <form @submit.prevent="handleSubmit">
      <label for="email">Email</label>
      <input
        id="email"
        v-model="email"
        type="email"
        autocomplete="username"
        :aria-invalid="invalid || undefined"
        :aria-describedby="invalid ? 'login-error' : undefined"
        @input="clearInvalid"
      />
      <label for="password">Password</label>
      <input
        id="password"
        v-model="password"
        type="password"
        autocomplete="current-password"
        :aria-invalid="invalid || undefined"
        :aria-describedby="invalid ? 'login-error' : undefined"
        @input="clearInvalid"
      />
      <small v-if="invalid" id="login-error">Invalid email or password.</small>
      <button type="submit">Log in</button>
      <p style="margin-top: 1rem">Don't have an account? <NuxtLink to="/register">Register</NuxtLink></p>
    </form>
  </article>
</template>
