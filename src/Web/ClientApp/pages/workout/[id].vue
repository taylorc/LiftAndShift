<template>
  <main class="container">
    <div v-if="loading" aria-busy="true">Loading...</div>

    <div v-else-if="store.currentWorkout">
      <hgroup>
        <h1>{{ formatDate(store.currentWorkout.date) }}</h1>
        <p>
          {{ store.currentWorkout.status }}
          <span v-if="store.currentWorkout.isProgrammeSession"> · Programme Session</span>
        </p>
      </hgroup>

      <div v-if="store.currentWorkout.notes" style="margin-bottom: 1rem;">
        <p>{{ store.currentWorkout.notes }}</p>
      </div>

      <article v-for="ex in store.currentWorkout.exercises" :key="ex.id">
        <header><strong>{{ ex.exerciseName }}</strong></header>
        <table role="grid">
          <thead>
            <tr><th>#</th><th>Type</th><th>Weight (kg)</th><th>Reps</th><th>Done</th><th>Completed</th></tr>
          </thead>
          <tbody>
            <tr v-for="s in ex.sets" :key="s.id">
              <td>{{ s.setNumber }}</td>
              <td>{{ s.setType }}</td>
              <td>{{ s.weightKg }}</td>
              <td>{{ s.reps }}</td>
              <td>{{ s.completedReps ?? '-' }}</td>
              <td>{{ s.isCompleted ? '✓' : '—' }}</td>
            </tr>
          </tbody>
        </table>
      </article>

      <div style="display: flex; gap: 1rem; margin-top: 1rem;">
        <button
          v-if="store.currentWorkout.status !== 'Completed'"
          @click="completeWorkout"
          :aria-busy="completing"
        >Mark Complete</button>
        <button class="outline" @click="duplicate" :aria-busy="duplicating">
          Duplicate as New Draft
        </button>
        <NuxtLink to="/history" class="outline secondary" role="button">Back to History</NuxtLink>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import { useWorkoutStore } from '~/stores/workout'

definePageMeta({ middleware: 'auth' })

const route = useRoute()
const router = useRouter()
const store = useWorkoutStore()
const loading = ref(true)
const completing = ref(false)
const duplicating = ref(false)

onMounted(async () => {
  await store.fetchWorkout(Number(route.params.id))
  loading.value = false
})

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleString()
}

async function completeWorkout() {
  completing.value = true
  await store.completeWorkout(store.currentWorkout!.id)
  completing.value = false
}

async function duplicate() {
  duplicating.value = true
  const newId = await store.duplicateWorkout(store.currentWorkout!.id)
  router.push('/workout/' + newId)
  duplicating.value = false
}
</script>
