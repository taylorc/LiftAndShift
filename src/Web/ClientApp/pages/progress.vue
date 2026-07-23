<template>
  <main class="container">
    <hgroup>
      <h1>Progress</h1>
      <p>Track your strength over time.</p>
    </hgroup>

    <div style="display: flex; gap: 1rem; align-items: flex-end; margin-bottom: 1.5rem; flex-wrap: wrap;">
      <label style="flex: 1; min-width: 200px; margin: 0;">
        Exercise
        <select v-model="selectedExerciseId" @change="loadProgress">
          <option value="">Select an exercise...</option>
          <option v-for="ex in exerciseStore.exercises" :key="ex.id" :value="ex.id">
            {{ ex.name }}
          </option>
        </select>
      </label>
      <label style="margin: 0;">
        Metric
        <select v-model="metric" style="width: auto;">
          <option value="estimated1Rm">Estimated 1RM</option>
          <option value="maxWeightKg">Max Weight</option>
          <option value="totalVolumeKg">Total Volume</option>
        </select>
      </label>
    </div>

    <div v-if="!selectedExerciseId" class="secondary">Select an exercise to view your progress.</div>

    <div v-else-if="store.loading" aria-busy="true">Loading...</div>

    <div v-else-if="!store.progress.length" class="secondary">
      No completed sessions with this exercise yet.
    </div>

    <div v-else>
      <ProgressChart :data="store.progress" :y-field="metric" />

      <table role="grid" style="margin-top: 1rem;">
        <thead>
          <tr><th>Date</th><th>Max Weight (kg)</th><th>Est. 1RM (kg)</th><th>Volume (kg)</th></tr>
        </thead>
        <tbody>
          <tr v-for="pt in [...store.progress].reverse()" :key="pt.date">
            <td>{{ formatDate(pt.date) }}</td>
            <td>{{ pt.maxWeightKg }}</td>
            <td>{{ pt.estimated1Rm.toFixed(1) }}</td>
            <td>{{ pt.totalVolumeKg.toFixed(1) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </main>
</template>

<script setup lang="ts">
import { useExerciseStore } from '~/stores/exercise'
import { useWorkoutStore } from '~/stores/workout'

definePageMeta({ middleware: 'auth' })

const exerciseStore = useExerciseStore()
const store = useWorkoutStore()
const selectedExerciseId = ref<number | ''>('')
const metric = ref<'estimated1Rm' | 'maxWeightKg' | 'totalVolumeKg'>('estimated1Rm')

onMounted(() => exerciseStore.fetchExercises())

async function loadProgress() {
  if (selectedExerciseId.value) {
    await store.fetchExerciseProgress(Number(selectedExerciseId.value))
  }
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString()
}
</script>
