<template>
  <main class="container">
    <hgroup>
      <h1>Exercise Library</h1>
      <p>All available exercises.</p>
    </hgroup>

    <!-- Filters -->
    <div style="display: flex; gap: 0.75rem; flex-wrap: wrap; margin-bottom: 1rem;">
      <input v-model="search" type="search" placeholder="Search..." style="flex: 1; min-width: 180px;" @input="applyFilters" />
      <select v-model="muscleGroupFilter" @change="applyFilters" style="width: auto;">
        <option value="">All muscle groups</option>
        <option v-for="mg in muscleGroups" :key="mg" :value="mg">{{ mg }}</option>
      </select>
    </div>

    <div v-if="store.loading" aria-busy="true">Loading...</div>

    <table v-else role="grid">
      <thead>
        <tr>
          <th>Name</th>
          <th>Muscle Group</th>
          <th>Equipment</th>
          <th>Movement</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="ex in store.exercises" :key="ex.id">
          <td><strong>{{ ex.name }}</strong><br v-if="ex.description" /><small v-if="ex.description">{{ ex.description }}</small></td>
          <td>{{ ex.muscleGroup }}</td>
          <td>{{ ex.equipmentType }}</td>
          <td>{{ ex.movementPattern }}</td>
          <td>
            <button v-if="ex.isCustom" class="outline secondary" style="padding: 0.25rem 0.5rem; margin: 0;" @click="deleteExercise(ex.id)">
              Delete
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Add custom -->
    <details style="margin-top: 2rem;">
      <summary>Add Custom Exercise</summary>
      <form @submit.prevent="addCustom" style="margin-top: 1rem;">
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
          <label>
            Name *
            <input v-model="form.name" required />
          </label>
          <label>
            Muscle Group
            <select v-model.number="form.muscleGroup">
              <option v-for="(mg, i) in muscleGroups" :key="i" :value="i">{{ mg }}</option>
            </select>
          </label>
          <label>
            Equipment
            <select v-model.number="form.equipmentType">
              <option v-for="(eq, i) in equipmentTypes" :key="i" :value="i">{{ eq }}</option>
            </select>
          </label>
          <label>
            Movement Pattern
            <select v-model.number="form.movementPattern">
              <option v-for="(mp, i) in movementPatterns" :key="i" :value="i">{{ mp }}</option>
            </select>
          </label>
        </div>
        <label>
          Description (optional)
          <input v-model="form.description" />
        </label>
        <button type="submit">Add Exercise</button>
      </form>
    </details>
  </main>
</template>

<script setup lang="ts">
import { useExerciseStore } from '~/stores/exercise'

definePageMeta({ middleware: 'auth' })

const store = useExerciseStore()
const search = ref('')
const muscleGroupFilter = ref('')

const muscleGroups = ['Legs', 'Back', 'Chest', 'Shoulders', 'Arms', 'Core', 'Full']
const equipmentTypes = ['Barbell', 'Dumbbell', 'Bodyweight', 'Machine', 'Cable', 'Kettlebell']
const movementPatterns = ['Squat', 'Hinge', 'Push', 'Pull', 'Carry']

const form = reactive({
  name: '',
  description: '',
  muscleGroup: 0,
  equipmentType: 0,
  movementPattern: 0,
})

onMounted(() => applyFilters())

async function applyFilters() {
  const mgIndex = muscleGroupFilter.value ? muscleGroups.indexOf(muscleGroupFilter.value) : undefined
  await store.fetchExercises(search.value || undefined, mgIndex !== -1 ? mgIndex : undefined)
}

async function deleteExercise(id: number) {
  if (confirm('Delete this exercise?')) {
    await store.deleteExercise(id)
  }
}

async function addCustom() {
  await store.createExercise({
    name: form.name,
    description: form.description || null,
    muscleGroup: form.muscleGroup,
    equipmentType: form.equipmentType,
    movementPattern: form.movementPattern,
  })
  form.name = ''
  form.description = ''
}
</script>
