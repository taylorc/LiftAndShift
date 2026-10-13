<script setup lang="ts">
import type { LiftName, WorkoutName } from '~/types'

defineProps<{ workoutLifts: Record<WorkoutName, LiftName[]> }>()
const emit = defineEmits<{ choose: [workout: WorkoutName] }>()

const workouts: WorkoutName[] = ['A', 'B']
</script>

<template>
  <div class="mt-8 flex gap-4 border-t border-chalk pt-8">
    <button
      v-for="workout in workouts"
      :key="workout"
      type="button"
      class="flex-1 border border-ink py-8 text-center transition-colors hover:border-iron hover:text-iron"
      @click="emit('choose', workout)"
    >
      <span class="font-display text-6xl font-black">{{ workout }}</span>
      <span class="mt-1 block text-sm text-ink/60">{{
        workoutLifts[workout].map(formatLiftName).join(' · ')
      }}</span>
    </button>
  </div>
</template>
