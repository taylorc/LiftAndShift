<script setup lang="ts">
import type { Lift } from '~/types'

const identity = useIdentityStore()
const api = useApi()
const { fetchWorkWeights } = useWorkWeights()

if (!identity.familyMemberId) {
  await navigateTo('/')
}

const { data: lifts } = await useAsyncData('lifts', () => api<Lift[]>('/Lifts'))

const workWeights = ref<Record<string, number | null>>({})

async function loadWorkWeights() {
  if (!lifts.value) return
  workWeights.value = await fetchWorkWeights(lifts.value.map((lift) => lift.name))
}

await loadWorkWeights()

const rampingLiftId = ref<number | null>(null)
const warmingUpLiftId = ref<number | null>(null)

function startRamp(lift: Lift) {
  warmingUpLiftId.value = null
  rampingLiftId.value = lift.id
}

async function finishRamp() {
  rampingLiftId.value = null
  await loadWorkWeights()
}

function toggleWarmUp(lift: Lift) {
  if (warmingUpLiftId.value === lift.id) {
    warmingUpLiftId.value = null
    return
  }

  rampingLiftId.value = null
  warmingUpLiftId.value = lift.id
}
</script>

<template>
  <div class="max-w-xl">
    <h1 class="font-display text-5xl font-black tracking-tight">Work Weights</h1>
    <p class="mt-1 text-sm text-ink/60">
      Training as {{ identity.name }}. Ramp a lift the first time you do it; after that, check its warm-up here.
    </p>

    <ul class="mt-8 border-t border-chalk">
      <template v-for="lift in lifts ?? []" :key="lift.id">
        <li
          class="border-b border-chalk border-l-4"
          :class="(rampingLiftId === lift.id || warmingUpLiftId === lift.id) ? 'border-l-iron' : 'border-l-transparent'"
        >
          <div class="flex items-center justify-between py-4 pl-4 pr-2">
            <span class="font-display text-2xl font-bold">{{ formatLiftName(lift.name) }}</span>

            <button
              v-if="workWeights[lift.name] != null"
              type="button"
              class="font-display text-2xl font-bold text-iron transition-colors hover:text-iron-dark"
              @click="toggleWarmUp(lift)"
            >
              {{ workWeights[lift.name] }}kg
            </button>
            <button
              v-else-if="rampingLiftId !== lift.id"
              type="button"
              class="border border-chalk px-3 py-1 text-sm text-ink/70 transition-colors hover:border-iron hover:text-iron"
              @click="startRamp(lift)"
            >
              Ramp this lift
            </button>
          </div>

          <RampPanel
            v-if="rampingLiftId === lift.id"
            :lift="lift"
            @complete="finishRamp"
            @cancel="rampingLiftId = null"
          />

          <WarmUpPanel
            v-if="warmingUpLiftId === lift.id"
            :lift="lift"
            :work-weight-kg="workWeights[lift.name] ?? null"
          />
        </li>
      </template>
    </ul>
  </div>
</template>
