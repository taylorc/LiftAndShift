<script setup lang="ts">
import type { LiftOutcome } from '~/types'

defineProps<{ outcomes: LiftOutcome[] }>()
const emit = defineEmits<{ 'log-another': [] }>()
</script>

<template>
  <div class="mt-8 border-t border-chalk pt-6">
    <p class="text-ink/70">Session logged.</p>
    <ul class="mt-4">
      <li
        v-for="outcome in outcomes"
        :key="outcome.lift"
        class="border-b border-chalk border-l-4 py-4 pl-4"
        :class="outcome.successful ? 'border-l-iron' : 'border-l-terracotta'"
      >
        <div class="flex items-center justify-between">
          <span class="font-display text-2xl font-bold">{{ formatLiftName(outcome.lift) }}</span>
          <span class="font-display text-2xl font-bold" :class="outcome.successful ? 'text-iron' : 'text-terracotta'">
            {{ outcome.newWeightKg }}kg
          </span>
        </div>
        <p class="mt-1 text-sm text-ink/60">
          <span v-if="outcome.successful">Every set hit target — Work Weight goes up next time.</span>
          <span v-else-if="outcome.deloaded">Second miss in a row — Work Weight comes down next time.</span>
          <span v-else>Missed a set — one more miss and Work Weight comes down.</span>
        </p>
      </li>
    </ul>
    <button
      type="button"
      class="mt-6 border border-ink px-6 py-2 font-medium text-ink transition-colors hover:border-iron hover:text-iron"
      @click="emit('log-another')"
    >
      Log another workout
    </button>
  </div>
</template>
