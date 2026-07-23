<template>
  <main class="container">
    <hgroup>
      <h1>Log Workout</h1>
      <p>Add exercises and track your sets.</p>
    </hgroup>

    <form @submit.prevent="save(false)">
      <label>
        Date
        <input v-model="date" type="datetime-local" required />
      </label>
      <label>
        Notes (optional)
        <textarea v-model="notes" rows="2"></textarea>
      </label>

      <!-- Exercise picker -->
      <div style="display: flex; gap: 0.5rem; margin-bottom: 1rem;">
        <select v-model="selectedExerciseId" style="flex: 1;">
          <option value="">Select an exercise...</option>
          <option v-for="ex in exerciseStore.exercises" :key="ex.id" :value="ex.id">
            {{ ex.name }} ({{ ex.muscleGroup }})
          </option>
        </select>
        <button type="button" @click="addExercise" :disabled="!selectedExerciseId">
          Add Exercise
        </button>
      </div>

      <!-- Exercise rows -->
      <WorkoutExerciseRow
        v-for="(ex, idx) in exercises"
        :key="idx"
        :exercise="ex"
        :index="idx"
        @update="exercises[idx] = $event"
        @remove="exercises.splice(idx, 1)"
        style="margin-bottom: 1rem;"
      />

      <p v-if="!exercises.length" class="secondary">No exercises added yet.</p>

      <div style="display: flex; gap: 1rem;">
        <button type="submit" class="outline">Save as Draft</button>
        <button type="button" @click="save(true)">Complete Workout</button>
      </div>

      <p v-if="error" style="color: var(--pico-del-color);">{{ error }}</p>
    </form>
  </main>
</template>

<script setup lang="ts">
import { useExerciseStore } from '~/stores/exercise'
import { useWorkoutStore } from '~/stores/workout'
import type { LogWorkoutExerciseDto } from '~/lib/web-api-client'

definePageMeta({ middleware: 'auth' })

const exerciseStore = useExerciseStore()
const workoutStore = useWorkoutStore()
const router = useRouter()

const date = ref(new Date().toISOString().slice(0, 16))
const notes = ref('')
const selectedExerciseId = ref<number | ''>('')
const exercises = ref<(LogWorkoutExerciseDto & { exerciseName?: string })[]>([])
const error = ref('')

onMounted(() => exerciseStore.fetchExercises())

function addExercise() {
  if (!selectedExerciseId.value) return
  const ex = exerciseStore.exercises.find(e => e.id === Number(selectedExerciseId.value))
  if (!ex) return

  exercises.value.push({
    exerciseId: ex.id,
    orderIndex: exercises.value.length,
    notes: null,
    exerciseName: ex.name,
    sets: [{
      setNumber: 1,
      setType: 1,
      weightKg: 0,
      reps: 5,
      completedReps: null,
      notes: null,
      isCompleted: false,
    }],
  })
  selectedExerciseId.value = ''
}

async function save(complete: boolean) {
  error.value = ''
  try {
    const id = await workoutStore.logWorkout({
      date: new Date(date.value).toISOString(),
      notes: notes.value || null,
      complete,
      exercises: exercises.value.map(({ exerciseName, ...rest }) => rest),
    })
    router.push('/workout/' + id)
  } catch (e: any) {
    error.value = e?.message ?? 'Failed to save workout'
  }
}
</script>
