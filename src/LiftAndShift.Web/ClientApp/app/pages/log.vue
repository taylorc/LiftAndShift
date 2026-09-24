<script setup lang="ts">
interface Programme {
  trainingPhase: number
}

interface Lift {
  name: string
  workSetCount: number
}

interface WorkWeight {
  weightKg: number
}

interface LiftOutcome {
  lift: string
  successful: boolean
  newWeightKg: number
  deloaded: boolean
}

interface WorkoutSession {
  liftOutcomes: LiftOutcome[]
}

const identity = useIdentityStore()
const api = useApi()

if (!identity.familyMemberId) {
  await navigateTo('/')
}

const { data: programme } = await useAsyncData('programme-for-log', () =>
  api<Programme>(`/FamilyMembers/${identity.familyMemberId}/Programme`)
)
const { data: lifts } = await useAsyncData('lifts-for-log', () => api<Lift[]>('/Lifts'))

const workout = ref<'A' | 'B' | null>(null)
const performedOn = ref(new Date().toISOString().slice(0, 10))
const repsBySet = ref<Record<string, number[]>>({})
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
  const entries = await Promise.all(
    names.map(async (name) => {
      try {
        const workWeight = await api<WorkWeight>(`/FamilyMembers/${identity.familyMemberId}/WorkWeights/${name}`)
        return [name, workWeight.weightKg] as const
      } catch {
        return [name, null] as const
      }
    })
  )
  workWeights.value = Object.fromEntries(entries)
  loadingWorkWeights.value = false

  repsBySet.value = {}
  for (const lift of liftsInWorkout.value) {
    repsBySet.value[lift.name] = Array.from({ length: lift.workSetCount }, () => 5)
  }
}

function changeWorkout() {
  workout.value = null
  result.value = null
}

async function submitSession() {
  if (!workout.value) return
  submitError.value = null
  submitting.value = true
  try {
    const response = await api<WorkoutSession>(`/FamilyMembers/${identity.familyMemberId}/WorkoutSessions`, {
      method: 'POST',
      body: {
        workout: workout.value,
        performedOn: performedOn.value,
        loggedLifts: liftsInWorkout.value.map((lift) => ({
          lift: lift.name,
          weightKg: workWeights.value[lift.name],
          repsPerSet: repsBySet.value[lift.name]
        }))
      }
    })
    result.value = response
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

    <div v-if="!workout" class="mt-8 flex gap-4 border-t border-chalk pt-8">
      <button
        type="button"
        class="flex-1 border border-ink py-8 text-center transition-colors hover:border-iron hover:text-iron"
        @click="chooseWorkout('A')"
      >
        <span class="font-display text-6xl font-black">A</span>
        <span class="mt-1 block text-sm text-ink/60">Squat · Press · Deadlift</span>
      </button>
      <button
        type="button"
        class="flex-1 border border-ink py-8 text-center transition-colors hover:border-iron hover:text-iron"
        @click="chooseWorkout('B')"
      >
        <span class="font-display text-6xl font-black">B</span>
        <span class="mt-1 block text-sm text-ink/60">Squat · Bench Press · {{ formatLiftName(liftsForWorkout('B', programme?.trainingPhase ?? 1)[2]) }}</span>
      </button>
    </div>

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

      <form v-else class="mt-6" @submit.prevent="submitSession">
        <label class="flex flex-col gap-1">
          <span class="text-sm font-medium text-ink/70">Date</span>
          <input
            v-model="performedOn"
            type="date"
            required
            class="w-48 border border-ink bg-paper px-3 py-2 focus:outline-2 focus:outline-iron"
          />
        </label>

        <div v-for="lift in liftsInWorkout" :key="lift.name" class="mt-8 border-t border-chalk pt-6">
          <div class="flex items-baseline justify-between">
            <span class="font-display text-2xl font-bold">{{ formatLiftName(lift.name) }}</span>
            <span class="font-display text-2xl font-bold text-iron">{{ workWeights[lift.name] }}kg</span>
          </div>
          <div class="mt-3 flex flex-wrap gap-4">
            <label
              v-for="(_, setIndex) in repsBySet[lift.name]"
              :key="setIndex"
              class="flex flex-col gap-1"
            >
              <span class="text-xs font-medium tracking-widest text-ink/50">SET {{ setIndex + 1 }}</span>
              <input
                v-model.number="repsBySet[lift.name][setIndex]"
                type="number"
                inputmode="numeric"
                min="0"
                max="20"
                required
                class="w-20 border border-ink bg-paper px-3 py-2 text-center focus:outline-2 focus:outline-iron"
              />
            </label>
          </div>
        </div>

        <button
          type="submit"
          :disabled="submitting"
          class="mt-8 bg-iron px-6 py-3 font-medium text-paper transition-colors hover:bg-iron-dark disabled:opacity-50"
        >
          Log Session
        </button>
        <p v-if="submitError" class="mt-3 text-sm text-terracotta">{{ submitError }}</p>
      </form>
    </template>

    <template v-else>
      <div class="mt-8 border-t border-chalk pt-6">
        <p class="text-ink/70">Session logged.</p>
        <ul class="mt-4">
          <li
            v-for="outcome in result.liftOutcomes"
            :key="outcome.lift"
            class="border-b border-chalk border-l-4 py-4 pl-4"
            :class="outcome.successful ? 'border-l-iron' : 'border-l-terracotta'"
          >
            <div class="flex items-center justify-between">
              <span class="font-display text-2xl font-bold">{{ formatLiftName(outcome.lift) }}</span>
              <span class="font-display text-2xl font-bold" :class="outcome.successful ? 'text-iron' : 'text-terracotta'">
                {{ outcome.newWeightKg }}kg
              </span>
            </div>
            <p class="mt-1 text-sm text-ink/60">
              <span v-if="outcome.successful">Every set hit target — Work Weight goes up next time.</span>
              <span v-else-if="outcome.deloaded">Second miss in a row — Work Weight comes down next time.</span>
              <span v-else>Missed a set — one more miss and Work Weight comes down.</span>
            </p>
          </li>
        </ul>
        <button
          type="button"
          class="mt-6 border border-ink px-6 py-2 font-medium text-ink transition-colors hover:border-iron hover:text-iron"
          @click="logAnother"
        >
          Log another workout
        </button>
      </div>
    </template>
  </div>
</template>
