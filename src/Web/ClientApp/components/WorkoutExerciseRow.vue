<template>
  <article>
    <header style="display: flex; justify-content: space-between; align-items: center;">
      <strong>{{ exercise.exerciseName || 'Exercise ' + (index + 1) }}</strong>
      <button type="button" class="outline secondary" style="padding: 0.25rem 0.75rem; margin: 0;" @click="$emit('remove')">
        Remove
      </button>
    </header>

    <table role="grid">
      <thead>
        <tr>
          <th>#</th>
          <th>Type</th>
          <th>Weight (kg)</th>
          <th>Target Reps</th>
          <th>Done Reps</th>
          <th>Done?</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <SetRow
          v-for="(set, si) in exercise.sets"
          :key="si"
          :set="set"
          @update="(field, value) => updateSet(si, field, value)"
          @remove="removeSet(si)"
        />
      </tbody>
    </table>

    <button type="button" class="outline" style="margin-top: 0.5rem;" @click="addSet">
      + Add Set
    </button>

    <div v-if="showTimer" style="margin-top: 0.5rem;">
      <RestTimer @done="showTimer = false" />
    </div>
  </article>
</template>

<script setup lang="ts">
import type { LogWorkoutExerciseDto, LogWorkoutSetDto } from '~/lib/web-api-client'

const props = defineProps<{
  exercise: LogWorkoutExerciseDto & { exerciseName?: string }
  index: number
}>()

const emit = defineEmits<{
  (e: 'update', exercise: LogWorkoutExerciseDto & { exerciseName?: string }): void
  (e: 'remove'): void
}>()

const showTimer = ref(false)

function addSet() {
  const lastSet = props.exercise.sets[props.exercise.sets.length - 1]
  const newSet: LogWorkoutSetDto = {
    setNumber: (props.exercise.sets.length) + 1,
    setType: 1,
    weightKg: lastSet?.weightKg ?? 0,
    reps: lastSet?.reps ?? 5,
    completedReps: null,
    notes: null,
    isCompleted: false,
  }
  emit('update', { ...props.exercise, sets: [...props.exercise.sets, newSet] })
}

function removeSet(index: number) {
  const sets = props.exercise.sets.filter((_, i) => i !== index)
    .map((s, i) => ({ ...s, setNumber: i + 1 }))
  emit('update', { ...props.exercise, sets })
}

function updateSet(index: number, field: string, value: any) {
  const sets = props.exercise.sets.map((s, i) =>
    i === index ? { ...s, [field]: value } : s
  )
  // Trigger timer when a set is marked complete
  if (field === 'isCompleted' && value === true) {
    showTimer.value = true
  }
  emit('update', { ...props.exercise, sets })
}
</script>
