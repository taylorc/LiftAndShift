<template>
  <table role="grid">
    <thead>
      <tr><th>#</th><th>Type</th><th>Weight (kg)</th><th>Target Reps</th><th>Completed Reps</th><th>Done</th></tr>
    </thead>
    <tbody>
      <tr v-for="(set, si) in sets" :key="si">
        <td>{{ set.setNumber }}</td>
        <td>{{ set.setType === 0 ? 'Warmup' : 'Working' }}</td>
        <td><input type="number" step="0.5" v-model.number="set.weightKg" /></td>
        <td><input type="number" v-model.number="set.reps" /></td>
        <td><input type="number" min="0" v-model.number="set.completedReps" /></td>
        <td><input type="checkbox" v-model="set.isCompleted" /></td>
      </tr>
    </tbody>
  </table>
</template>

<script setup lang="ts">
import type { LogWorkoutSetDto } from '~/lib/web-api-client'

// Sets are edited in place; the parent owns the array.
defineProps<{ sets: LogWorkoutSetDto[] }>()
</script>
