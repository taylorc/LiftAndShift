<template>
  <tr>
    <td>{{ set.setNumber }}</td>
    <td>
      <select :value="set.setType" @change="update('setType', Number(($event.target as HTMLSelectElement).value))" style="padding: 0.25rem; width: 100%;">
        <option value="0">Warmup</option>
        <option value="1">Working</option>
        <option value="2">Drop</option>
        <option value="3">AMRAP</option>
      </select>
    </td>
    <td>
      <input type="number" :value="set.weightKg" @change="update('weightKg', Number(($event.target as HTMLInputElement).value))"
        step="1.25" min="0" style="width: 80px;" />
    </td>
    <td>
      <input type="number" :value="set.reps" @change="update('reps', Number(($event.target as HTMLInputElement).value))"
        min="1" style="width: 60px;" />
    </td>
    <td>
      <input type="number" :value="set.completedReps ?? ''" @change="update('completedReps', ($event.target as HTMLInputElement).value ? Number(($event.target as HTMLInputElement).value) : null)"
        min="0" style="width: 60px;" placeholder="-" />
    </td>
    <td>
      <input type="checkbox" :checked="set.isCompleted" @change="update('isCompleted', ($event.target as HTMLInputElement).checked)" />
    </td>
    <td>
      <button type="button" class="outline secondary" style="padding: 0.25rem 0.5rem; margin: 0;" @click="$emit('remove')">
        <X :size="14" />
      </button>
    </td>
  </tr>
</template>

<script setup lang="ts">
import { X } from '@lucide/vue'
import type { LogWorkoutSetDto } from '~/lib/web-api-client'

const props = defineProps<{ set: LogWorkoutSetDto }>()
const emit = defineEmits<{ (e: 'update', field: string, value: any): void; (e: 'remove'): void }>()

function update(field: string, value: any) {
  emit('update', field, value)
}
</script>
