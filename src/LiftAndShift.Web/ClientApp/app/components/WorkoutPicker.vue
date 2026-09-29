<script setup lang="ts">
defineProps<{ trainingPhase: number }>()
const emit = defineEmits<{ choose: ['A' | 'B'] }>()
</script>

<template>
  <div class="mt-8 flex gap-4 border-t border-chalk pt-8">
    <button
      type="button"
      class="flex-1 border border-ink py-8 text-center transition-colors hover:border-iron hover:text-iron"
      @click="emit('choose', 'A')"
    >
      <span class="font-display text-6xl font-black">A</span>
      <span class="mt-1 block text-sm text-ink/60">Squat · Press · Deadlift</span>
    </button>
    <button
      type="button"
      class="flex-1 border border-ink py-8 text-center transition-colors hover:border-iron hover:text-iron"
      @click="emit('choose', 'B')"
    >
      <span class="font-display text-6xl font-black">B</span>
      <span class="mt-1 block text-sm text-ink/60"
        >Squat · Bench Press · {{ formatLiftName(liftsForWorkout('B', trainingPhase)[2]!) }}</span
      >
    </button>
  </div>
</template>
