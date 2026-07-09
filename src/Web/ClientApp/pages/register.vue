<script setup lang="ts">
const MIN_PASSWORD_LENGTH = 6

function validateEmail(value: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)
}

const auth = useAuthStore()
const router = useRouter()

const email = ref('')
const password = ref('')
const emailTouched = ref(false)
const passwordTouched = ref(false)
const error = ref('')

const emailValid = computed(() => validateEmail(email.value))
const passwordValid = computed(() => password.value.length >= MIN_PASSWORD_LENGTH)
const emailInvalid = computed(() => emailTouched.value ? !emailValid.value : undefined)
const passwordInvalid = computed(() => passwordTouched.value ? !passwordValid.value : undefined)

async function handleSubmit() {
  error.value = ''
  emailTouched.value = true
  passwordTouched.value = true
  if (!emailValid.value || !passwordValid.value) return
  try {
    await auth.register(email.value, password.value)
    router.push('/login')
  } catch {
    error.value = 'Registration failed. Please try again.'
  }
}
</script>

<template>
  <article>
    <h2>Register</h2>
    <p v-if="error" class="error">{{ error }}</p>
    <form @submit.prevent="handleSubmit">
      <label for="email">Email</label>
      <input
        id="email"
        v-model="email"
        type="email"
        autocomplete="username"
        :aria-invalid="emailInvalid"
        aria-describedby="email-helper"
        @blur="emailTouched = true"
      />
      <small id="email-helper">
        {{ emailTouched && !emailValid ? 'Please enter a valid email address.' : '' }}
      </small>
      <label for="password">Password</label>
      <input
        id="password"
        v-model="password"
        type="password"
        autocomplete="new-password"
        :aria-invalid="passwordInvalid"
        aria-describedby="password-helper"
        @blur="passwordTouched = true"
      />
      <small id="password-helper">
        {{ passwordTouched && !passwordValid ? `Password must be at least ${MIN_PASSWORD_LENGTH} characters.` : '' }}
      </small>
      <button type="submit">Register</button>
      <p style="margin-top: 1rem">Already have an account? <NuxtLink to="/login">Log in</NuxtLink></p>
    </form>
  </article>
</template>
