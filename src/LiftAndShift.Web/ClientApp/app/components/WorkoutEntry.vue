<script setup lang="ts">
import type { Lift, WorkoutName, WorkoutSessionInput, WorkWeightsByLift } from '~/types'

const props = defineProps<{
  workout: WorkoutName
  liftsInWorkout: Lift[]
  workWeights: WorkWeightsByLift
  loadingWorkWeights: boolean
  submitting: boolean
  submitError: string | null
}>()

const emit = defineEmits<{ change: []; submit: [payload: WorkoutSessionInput] }>()

const unrampedLifts = computed(() =>
  props.liftsInWorkout.filter((lift) => props.workWeights[lift.name] == null)
)
const readyToLog = computed(
  () => props.liftsInWorkout.length > 0 && unrampedLifts.value.length === 0
)
</script>

<template>
  <div>
    <div class="mt-8 flex items-center justify-between border-t border-chalk pt-6">
      <span class="font-display text-3xl font-black text-iron">Workout {{ workout }}</span>
      <button type="button" class="text-sm text-ink/60 hover:text-ink" @click="emit('change')">
        Change
      </button>
    </div>

    <p v-if="loadingWorkWeights" class="mt-6 text-ink/60">Loading your Work Weights…</p>

    <div v-else-if="unrampedLifts.length > 0" class="mt-6 border-l-4 border-l-terracotta pl-4">
      <p class="text-ink/80">
        You haven't Ramped
        <strong class="font-semibold">{{
          unrampedLifts.map((l) => formatLiftName(l.name)).join(', ')
        }}</strong>
        yet, so this workout can't be logged.
      </p>
      <NuxtLink
        to="/work-weights"
        class="mt-2 inline-block text-sm font-medium text-iron hover:text-iron-dark"
      >
        Go Ramp it →
      </NuxtLink>
    </div>

    <WorkoutForm
      v-else-if="readyToLog"
      :lifts-in-workout="liftsInWorkout"
      :work-weights="workWeights"
      :submitting="submitting"
      :submit-error="submitError"
      @submit="emit('submit', $event)"
    />
  </div>
</template>
