<template>
  <main class="container">
    <hgroup>
      <h1>Workout {{ store.nextSession?.workoutType }}</h1>
      <p>Pre-filled from your programme. Adjust anything before completing the session.</p>
    </hgroup>

    <div v-if="loading" aria-busy="true">Loading...</div>

    <div v-else-if="!store.nextSession">
      <p>No scheduled session was found.</p>
      <button @click="router.push('/programme')">Back to Programme</button>
    </div>

    <form v-else @submit.prevent="save">
      <article v-for="(ex, idx) in exercises" :key="idx">
        <header style="display: flex; justify-content: space-between; align-items: center;">
          <strong>{{ ex.liftName }}</strong>
          <label v-if="!ex.exerciseId" style="margin: 0;">
            <select v-model.number="ex.exerciseId">
              <option value="">Match to exercise...</option>
              <option v-for="opt in exerciseStore.exercises" :key="opt.id" :value="opt.id">
                {{ opt.name }}
              </option>
            </select>
          </label>
        </header>

        <table role="grid">
          <thead>
            <tr><th>#</th><th>Type</th><th>Weight (kg)</th><th>Target Reps</th><th>Completed Reps</th><th>Done</th></tr>
          </thead>
          <tbody>
            <tr v-for="(set, si) in ex.sets" :key="si">
              <td>{{ set.setNumber }}</td>
              <td>{{ set.setType === 0 ? 'Warmup' : 'Working' }}</td>
              <td><input type="number" step="0.5" v-model.number="set.weightKg" /></td>
              <td><input type="number" v-model.number="set.reps" /></td>
              <td><input type="number" min="0" v-model.number="set.completedReps" /></td>
              <td><input type="checkbox" v-model="set.isCompleted" /></td>
            </tr>
          </tbody>
        </table>
      </article>

      <div style="display: flex; gap: 1rem;">
        <button type="submit" :aria-busy="saving" :disabled="!allExercisesMapped || !readyToComplete">Complete Session</button>
      </div>

      <p v-if="!allExercisesMapped" class="secondary">Match every lift to an exercise before completing.</p>
      <p v-if="!readyToComplete" class="secondary">Enter completed reps for every working set, and tick Done for each set you've logged.</p>
      <p v-if="error" style="color: var(--pico-del-color);">{{ error }}</p>
    </form>
  </main>
</template>

<script setup lang="ts">
import { useProgrammeStore } from '~/stores/programme'
import { useExerciseStore } from '~/stores/exercise'
import type { LogWorkoutExerciseDto, LogWorkoutSetDto } from '~/lib/web-api-client'

definePageMeta({ middleware: 'auth' })

const store = useProgrammeStore()
const exerciseStore = useExerciseStore()
const router = useRouter()

const loading = ref(true)
const saving = ref(false)
const error = ref('')

type PrefilledExercise = LogWorkoutExerciseDto & { liftName: string }
const exercises = ref<PrefilledExercise[]>([])

onMounted(async () => {
  if (!store.activeProgramme) {
    await store.fetchActiveProgramme()
  }
  await exerciseStore.fetchExercises()

  const nextSession = store.nextSession
  if (nextSession) {
    exercises.value = nextSession.prescribedLifts.map((lift, orderIndex) => {
      const matched = exerciseStore.exercises.find(
        (e) => e.name.trim().toLowerCase() === lift.liftName.trim().toLowerCase()
      )

      const warmupSets: LogWorkoutSetDto[] = lift.warmupSets.map((ws) => ({
        setNumber: ws.setNumber,
        setType: 0,
        weightKg: ws.weightKg,
        reps: ws.reps,
        completedReps: null,
        notes: null,
        isCompleted: false,
      }))

      // Weight/reps are pre-filled from the programme; the lifter ticks Done per set as they complete it.
      const workingSets: LogWorkoutSetDto[] = Array.from({ length: lift.sets }, (_, i) => ({
        setNumber: warmupSets.length + i + 1,
        setType: 1,
        weightKg: lift.weightKg,
        reps: lift.reps,
        completedReps: null,
        notes: null,
        isCompleted: false,
      }))

      return {
        liftName: lift.liftName,
        exerciseId: matched?.id ?? 0,
        orderIndex,
        notes: null,
        sets: [...warmupSets, ...workingSets],
      }
    })
  }

  loading.value = false
})

const allExercisesMapped = computed(() => exercises.value.every((ex) => ex.exerciseId))

// A working set must have its completed reps entered; any set with completed reps entered
// (working or warmup) must be ticked Done.
const readyToComplete = computed(() =>
  exercises.value.every((ex) =>
    ex.sets.every((set) => {
      const repsEntered = set.completedReps !== null && set.completedReps !== undefined
      if (set.setType === 1 && !repsEntered) return false
      if (repsEntered && !set.isCompleted) return false
      return true
    }),
  ),
)

async function save() {
  error.value = ''
  saving.value = true
  try {
    const workoutId = await store.logProgrammeSession(store.activeProgramme!.id, {
      programmeSessionId: store.nextSession!.sessionId,
      exercises: exercises.value.map(({ liftName, ...rest }) => rest),
    })
    router.push('/workout/' + workoutId)
  } catch (e: any) {
    error.value = e?.message ?? 'Failed to save session'
  } finally {
    saving.value = false
  }
}
</script>
