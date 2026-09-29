<script setup lang="ts">
import type { Lift, Programme, WorkoutSession } from '~/types'

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

const workout = ref<'A' | 'B' | null>(null)
const workWeights = ref<Record<string, number | null>>({})
const loadingWorkWeights = ref(false)
const submitting = ref(false)
const submitError = ref<string | null>(null)
const result = ref<WorkoutSession | null>(null)

const liftsInWorkout = computed(() => {
  if (!workout.value || !programme.value) return []
  const names = liftsForWorkout(workout.value, programme.value.trainingPhase)
  return names
    .map((name) => lifts.value?.find((l) => l.name === name))
    .filter((l): l is Lift => l != null)
})

const unrampedLifts = computed(() =>
  liftsInWorkout.value.filter((lift) => workWeights.value[lift.name] == null)
)

const readyToLog = computed(() => liftsInWorkout.value.length > 0 && unrampedLifts.value.length === 0)

async function chooseWorkout(choice: 'A' | 'B') {
  workout.value = choice
  result.value = null
  submitError.value = null
  loadingWorkWeights.value = true

  const names = liftsForWorkout(choice, programme.value!.trainingPhase)
  workWeights.value = await fetchWorkWeights(names)
  loadingWorkWeights.value = false
}

function changeWorkout() {
  workout.value = null
  result.value = null
}

async function submitSession(payload: {
  performedOn: string
  loggedLifts: { lift: string; weightKg: number; repsPerSet: number[] }[]
}) {
  if (!workout.value) return
  submitError.value = null
  submitting.value = true
  try {
    result.value = await api<WorkoutSession>(`/FamilyMembers/${identity.familyMemberId}/WorkoutSessions`, {
      method: 'POST',
      body: { workout: workout.value, ...payload }
    })
  } catch {
    submitError.value = "Couldn't log this session. Check your entries and try again."
  } finally {
    submitting.value = false
  }
}

function logAnother() {
  workout.value = null
  result.value = null
}
</script>

<template>
  <div class="max-w-xl">
    <h1 class="font-display text-5xl font-black tracking-tight">Log a Workout</h1>
    <p class="mt-1 text-sm text-ink/60">Training as {{ identity.name }}.</p>

    <WorkoutPicker v-if="!workout" :training-phase="programme?.trainingPhase ?? 1" @choose="chooseWorkout" />

    <template v-else-if="!result">
      <div class="mt-8 flex items-center justify-between border-t border-chalk pt-6">
        <span class="font-display text-3xl font-black text-iron">Workout {{ workout }}</span>
        <button type="button" class="text-sm text-ink/60 hover:text-ink" @click="changeWorkout">Change</button>
      </div>

      <p v-if="loadingWorkWeights" class="mt-6 text-ink/60">Loading your Work Weights…</p>

      <div v-else-if="unrampedLifts.length > 0" class="mt-6 border-l-4 border-l-terracotta pl-4">
        <p class="text-ink/80">
          You haven't Ramped
          <strong class="font-semibold">{{ unrampedLifts.map((l) => formatLiftName(l.name)).join(', ') }}</strong>
          yet, so this workout can't be logged.
        </p>
        <NuxtLink to="/work-weights" class="mt-2 inline-block text-sm font-medium text-iron hover:text-iron-dark">
          Go Ramp it →
        </NuxtLink>
      </div>

      <WorkoutForm
        v-else-if="readyToLog"
        :lifts-in-workout="liftsInWorkout"
        :work-weights="workWeights"
        :submitting="submitting"
        :submit-error="submitError"
        @submit="submitSession"
      />
    </template>

    <WorkoutResult v-else :outcomes="result.liftOutcomes" @log-another="logAnother" />
  </div>
</template>
