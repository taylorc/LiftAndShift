<template>
  <div>
    <p v-if="!records.length" class="secondary">No personal records yet. Complete a workout to set your first PRs.</p>
    <table v-else role="grid">
      <thead>
        <tr>
          <th>Exercise</th>
          <th>Weight (kg)</th>
          <th>Reps</th>
          <th>Est. 1RM (kg)</th>
          <th>Achieved</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="pr in records" :key="pr.exerciseId">
          <td><strong>{{ pr.exerciseName }}</strong></td>
          <td>{{ pr.weightKg }}</td>
          <td>{{ pr.reps }}</td>
          <td>{{ pr.estimated1RmKg.toFixed(1) }}</td>
          <td>{{ formatDate(pr.achievedAt) }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { PersonalRecordSummaryDto } from '~/lib/web-api-client'

defineProps<{ records: PersonalRecordSummaryDto[] }>()

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString()
}
</script>
