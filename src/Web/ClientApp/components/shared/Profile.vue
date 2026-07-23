<script setup lang="ts">
import { SaveUserOnboardingCommand } from '~/lib/web-api-client'



const props = defineProps<{ heading: string; subTitle: string }>()

const auth = useAuthStore()
const router = useRouter()

const error = ref('')
const isSubmitting = ref(false)
const isBeginner = ref(false)

const form = ref({
  preferredUnit: 'Lbs',
  bodyWeight: '',
  alternatingLift: 'PowerClean',
  squatStartingWeight: '',
  benchPressStartingWeight: '',
  overheadPressStartingWeight: '',
  deadliftStartingWeight: '',
  alternatingLiftStartingWeight: '',
})

const isLoading = ref(true);

onMounted(async () => {
  const client = useOnboardingClient()
  try {
    const data = await client.getOnboarding()
    if (data.isOnboarded) {
      isBeginner.value = true     
      form.value = {
        preferredUnit: data.preferredUnit ?? 'Lbs',
        bodyWeight: data.bodyWeight?.toString() ?? '',
        alternatingLift: data.alternatingLift ?? 'PowerClean',
        squatStartingWeight: data.squatStartingWeight?.toString() ?? '',
        benchPressStartingWeight: data.benchPressStartingWeight?.toString() ?? '',
        overheadPressStartingWeight: data.overheadPressStartingWeight?.toString() ?? '',
        deadliftStartingWeight: data.deadliftStartingWeight?.toString() ?? '',
        alternatingLiftStartingWeight: data.alternatingLiftStartingWeight?.toString() ?? ''
      }

    }
  } catch { /* ignore */ }
})

const unitLabel = computed(() => form.value.preferredUnit === 'Lbs' ? 'lbs' : 'kg')
const altLiftLabel = computed(() => form.value.alternatingLift === 'PowerClean' ? 'Power Clean' : 'Pendlay Row')

async function handleSubmit() {
  error.value = ''
  isSubmitting.value = true
  const client = useOnboardingClient()
  try {
    await client.saveOnboarding(new SaveUserOnboardingCommand({
      preferredUnit: form.value.preferredUnit,
      bodyWeight: parseFloat(form.value.bodyWeight),
      alternatingLift: form.value.alternatingLift,
      squatStartingWeight: parseFloat(form.value.squatStartingWeight),
      benchPressStartingWeight: parseFloat(form.value.benchPressStartingWeight),
      overheadPressStartingWeight: parseFloat(form.value.overheadPressStartingWeight),
      deadliftStartingWeight: parseFloat(form.value.deadliftStartingWeight),
      alternatingLiftStartingWeight: parseFloat(form.value.alternatingLiftStartingWeight),
    }))
    await auth.fetchOnboardingStatus()
    router.push('/')
  } catch {
    error.value = 'Failed to save. Please check your inputs and try again.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  
  <article>
    <h2>{{ heading }}</h2>
    <p>{{ subTitle }}</p>

    <p v-if="error" role="alert" style="color: var(--pico-color-red-500)">{{ error }}</p>

    <form @submit.prevent="handleSubmit">
      <fieldset>
        <legend><strong>Preferred Unit</strong></legend>
        <label>
          <input v-model="form.preferredUnit" type="radio" name="unit" value="Lbs" />
          Lbs
        </label>
        <label>
          <input v-model="form.preferredUnit" type="radio" name="unit" value="Kgs" />
          Kgs
        </label>
      </fieldset>

      <label for="bodyWeight">Body Weight ({{ unitLabel }})</label>
      <input id="bodyWeight" v-model="form.bodyWeight" type="number" min="0" step="0.1" required />

      <fieldset v-bind:hidden="!isBeginner">
        <legend><strong>Alternating Lift (Workout B)</strong></legend>
        <label>
          <input v-model="form.alternatingLift" type="radio" name="altLift" value="PowerClean" />
          Power Clean
        </label>
        <label>
          <input v-model="form.alternatingLift" type="radio" name="altLift" value="PendlayRow" />
          Pendlay Row
        </label>
      </fieldset>

      <h3>Starting Working Weights ({{ unitLabel }})</h3>

      <label for="squat">Squat</label>
      <input id="squat" v-model="form.squatStartingWeight" type="number" min="0" step="2.5" required />

      <label for="bench">Bench Press</label>
      <input id="bench" v-model="form.benchPressStartingWeight" type="number" min="0" step="2.5" required />

      <label for="ohp">Overhead Press</label>
      <input id="ohp" v-model="form.overheadPressStartingWeight" type="number" min="0" step="2.5" required />

      <label for="deadlift">Deadlift</label>
      <input id="deadlift" v-model="form.deadliftStartingWeight" type="number" min="0" step="2.5" required />

      <label for="altLiftWeight">{{ altLiftLabel }}</label>
      <input id="altLiftWeight" v-model="form.alternatingLiftStartingWeight" type="number" min="0" step="2.5" required />

      <button type="submit" :aria-busy="isSubmitting" :disabled="isSubmitting">
        {{ isSubmitting ? 'Saving…' : 'Start Training' }}
      </button>
    </form>
  </article>
</template>
