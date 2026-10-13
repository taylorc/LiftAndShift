<script setup lang="ts">
import type {
  Lift,
  Programme,
  WorkoutName,
  WorkoutSession,
  WorkoutSessionInput,
  WorkWeightsByLift
} from '~/types'

const identity = useIdentityStore()
const api = useApi()
const { fetchWorkWeights } = useWorkWeights()

if (!identity.familyMemberId) {
  await navigateTo('/')
}

const { data: programme } = await useAsyncData('programme-for-log', () =>
  api<Programme>(`/FamilyMembers/${identity.familyMemberId}/Programme`)
)
const { data: lifts } = await useAsyncData('lifts-for-log', () => api<Lift[]>('/Lifts'))

const workout = ref<WorkoutName | null>(null)
const workWeights = ref<WorkWeightsByLift>({})
const loadingWorkWeights = ref(false)
const submitting = ref(false)
const submitError = ref<string | null>(null)
const result = ref<WorkoutSession | null>(null)

const liftsInWorkout = computed(() => {
  if (!workout.value || !programme.value) return []
  return programme.value.workoutLifts[workout.value]
    .map((name) => lifts.value?.find((l) => l.name === name))
    .filter((l): l is Lift => l != null)
})

async function chooseWorkout(choice: WorkoutName) {
  workout.value = choice
  result.value = null
  submitError.value = null
  loadingWorkWeights.value = true

  workWeights.value = await fetchWorkWeights(programme.value!.workoutLifts[choice])
  loadingWorkWeights.value = false
}

function changeWorkout() {
  workout.value = null
  result.value = null
}

async function submitSession(payload: WorkoutSessionInput) {
  if (!workout.value) return
  submitError.value = null
  submitting.value = true
  try {
    result.value = await api<WorkoutSession>(
      `/FamilyMembers/${identity.familyMemberId}/WorkoutSessions`,
      {
        method: 'POST',
        body: { workout: workout.value, ...payload }
      }
    )
  } catch {
    submitError.value = "Couldn't log this session. Check your entries and try again."
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="max-w-xl">
    <h1 class="font-display text-5xl font-black tracking-tight">Log a Workout</h1>
    <p class="mt-1 text-sm text-ink/60">Training as {{ identity.name }}.</p>

    <template v-if="programme">
      <WorkoutPicker
        v-if="!workout"
        :workout-lifts="programme.workoutLifts"
        @choose="chooseWorkout"
      />

      <WorkoutEntry
        v-else-if="!result"
        :workout="workout"
        :lifts-in-workout="liftsInWorkout"
        :work-weights="workWeights"
        :loading-work-weights="loadingWorkWeights"
        :submitting="submitting"
        :submit-error="submitError"
        @change="changeWorkout"
        @submit="submitSession"
      />

      <WorkoutResult v-else :outcomes="result.liftOutcomes" @log-another="changeWorkout" />
    </template>
  </div>
</template>
