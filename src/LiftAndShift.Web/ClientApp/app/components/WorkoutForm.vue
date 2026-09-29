<script setup lang="ts">
import type { Lift } from '~/types'

const props = defineProps<{
  liftsInWorkout: Lift[]
  workWeights: Record<string, number | null>
  submitting: boolean
  submitError: string | null
}>()

const emit = defineEmits<{
  submit: [payload: { performedOn: string; loggedLifts: { lift: string; weightKg: number; repsPerSet: number[] }[] }]
}>()

const performedOn = ref(new Date().toISOString().slice(0, 10))
const repsBySet = ref<Record<string, number[]>>({})

watch(
  () => props.liftsInWorkout,
  (lifts) => {
    repsBySet.value = Object.fromEntries(
      lifts.map((lift) => [lift.name, Array.from({ length: lift.workSetCount }, () => 5)])
    )
  },
  { immediate: true }
)

function submit() {
  emit('submit', {
    performedOn: performedOn.value,
    loggedLifts: props.liftsInWorkout.map((lift) => ({
      lift: lift.name,
      weightKg: props.workWeights[lift.name]!,
      repsPerSet: repsBySet.value[lift.name]!
    }))
  })
}
</script>

<template>
  <form class="mt-6" @submit.prevent="submit">
    <label class="flex flex-col gap-1">
      <span class="text-sm font-medium text-ink/70">Date</span>
      <input
        v-model="performedOn"
        type="date"
        required
        class="w-48 border border-ink bg-paper px-3 py-2 focus:outline-2 focus:outline-iron"
      />
    </label>

    <div v-for="lift in liftsInWorkout" :key="lift.name" class="mt-8 border-t border-chalk pt-6">
      <div class="flex items-baseline justify-between">
        <span class="font-display text-2xl font-bold">{{ formatLiftName(lift.name) }}</span>
        <span class="font-display text-2xl font-bold text-iron">{{ workWeights[lift.name] }}kg</span>
      </div>
      <div class="mt-3 flex flex-wrap gap-4">
        <label v-for="(_, setIndex) in repsBySet[lift.name]" :key="setIndex" class="flex flex-col gap-1">
          <span class="text-xs font-medium tracking-widest text-ink/50">SET {{ setIndex + 1 }}</span>
          <input
            v-model.number="repsBySet[lift.name]![setIndex]"
            type="number"
            inputmode="numeric"
            min="0"
            max="20"
            required
            class="w-20 border border-ink bg-paper px-3 py-2 text-center focus:outline-2 focus:outline-iron"
          />
        </label>
      </div>
    </div>

    <button
      type="submit"
      :disabled="submitting"
      class="mt-8 bg-iron px-6 py-3 font-medium text-paper transition-colors hover:bg-iron-dark disabled:opacity-50"
    >
      Log Session
    </button>
    <p v-if="submitError" class="mt-3 text-sm text-terracotta">{{ submitError }}</p>
  </form>
</template>
