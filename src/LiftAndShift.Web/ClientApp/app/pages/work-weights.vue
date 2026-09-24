<script setup lang="ts">
interface Lift {
  id: number
  name: string
  openingWeightKg: number
  incrementKg: number
}

interface WorkWeight {
  weightKg: number
}

interface WarmUpSet {
  weightKg: number
  reps: number
}

const identity = useIdentityStore()
const api = useApi()

if (!identity.familyMemberId) {
  await navigateTo('/')
}

const { data: lifts } = await useAsyncData('lifts', () => api<Lift[]>('/Lifts'))

const workWeights = ref<Record<string, number | null>>({})

async function loadWorkWeights() {
  if (!lifts.value) return
  const entries = await Promise.all(
    lifts.value.map(async (lift) => {
      try {
        const workWeight = await api<WorkWeight>(`/FamilyMembers/${identity.familyMemberId}/WorkWeights/${lift.name}`)
        return [lift.name, workWeight.weightKg] as const
      } catch {
        return [lift.name, null] as const
      }
    })
  )
  workWeights.value = Object.fromEntries(entries)
}

await loadWorkWeights()

const rampingLift = ref<Lift | null>(null)
const currentSet = ref(1)
const rampError = ref<string | null>(null)
const submitting = ref(false)

const currentSetWeight = computed(() => {
  if (!rampingLift.value) return 0
  return rampingLift.value.openingWeightKg + (currentSet.value - 1) * rampingLift.value.incrementKg
})

function startRamp(lift: Lift) {
  warmingUpLift.value = null
  rampingLift.value = lift
  currentSet.value = 1
  rampError.value = null
}

function cancelRamp() {
  rampingLift.value = null
}

function nextSet() {
  currentSet.value += 1
}

async function finishRamp() {
  if (!rampingLift.value) return
  rampError.value = null
  submitting.value = true
  try {
    await api(`/FamilyMembers/${identity.familyMemberId}/WorkWeights/${rampingLift.value.name}/CompleteRamp`, {
      method: 'POST',
      body: { finalSetNumber: currentSet.value }
    })
    rampingLift.value = null
    await loadWorkWeights()
  } catch {
    rampError.value = "Couldn't record your Work Weight. Try again."
  } finally {
    submitting.value = false
  }
}

const warmingUpLift = ref<Lift | null>(null)
const warmUpSets = ref<WarmUpSet[]>([])
const warmUpLoading = ref(false)

async function toggleWarmUp(lift: Lift) {
  if (warmingUpLift.value?.id === lift.id) {
    warmingUpLift.value = null
    return
  }

  rampingLift.value = null
  warmingUpLift.value = lift
  warmUpLoading.value = true
  try {
    warmUpSets.value = await api<WarmUpSet[]>(`/FamilyMembers/${identity.familyMemberId}/WorkWeights/${lift.name}/WarmUp`)
  } finally {
    warmUpLoading.value = false
  }
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
          :class="(rampingLift?.id === lift.id || warmingUpLift?.id === lift.id) ? 'border-l-iron' : 'border-l-transparent'"
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
              v-else-if="rampingLift?.id !== lift.id"
              type="button"
              class="border border-chalk px-3 py-1 text-sm text-ink/70 transition-colors hover:border-iron hover:text-iron"
              @click="startRamp(lift)"
            >
              Ramp this lift
            </button>
          </div>

          <div v-if="rampingLift?.id === lift.id" class="pb-6 pl-4 pr-2">
            <span class="text-sm font-medium tracking-widest text-ink/50">SET {{ currentSet }}</span>
            <div class="font-display text-7xl font-black leading-none text-iron">{{ currentSetWeight }}kg</div>
            <p class="mt-2 text-sm text-ink/60">Load the bar to this weight for 5 reps.</p>

            <div class="mt-4 flex flex-wrap gap-3">
              <button
                type="button"
                :disabled="submitting"
                class="border border-ink px-5 py-2 font-medium text-ink transition-colors hover:border-iron hover:text-iron disabled:opacity-50"
                @click="nextSet"
              >
                Bar's still moving — next set
              </button>
              <button
                type="button"
                :disabled="submitting"
                class="bg-iron px-5 py-2 font-medium text-paper transition-colors hover:bg-iron-dark disabled:opacity-50"
                @click="finishRamp"
              >
                This is it — bar slowed
              </button>
              <button
                type="button"
                class="px-3 py-2 text-sm text-ink/60 hover:text-ink"
                @click="cancelRamp"
              >
                Cancel
              </button>
            </div>
            <p v-if="rampError" class="mt-3 text-sm text-terracotta">{{ rampError }}</p>
          </div>

          <div v-if="warmingUpLift?.id === lift.id" class="pb-6 pl-4 pr-2">
            <span class="text-sm font-medium tracking-widest text-ink/50">WARM-UP</span>
            <p v-if="warmUpLoading" class="mt-2 text-ink/60">Loading…</p>
            <ol v-else class="mt-2 divide-y divide-chalk">
              <li v-for="(set, index) in warmUpSets" :key="index" class="flex items-baseline justify-between py-2">
                <span class="text-sm text-ink/50">Set {{ index + 1 }}</span>
                <span class="font-display text-xl font-bold">{{ set.weightKg }}kg <span class="text-sm font-normal text-ink/50">× {{ set.reps }}</span></span>
              </li>
              <li class="flex items-baseline justify-between py-2">
                <span class="text-sm text-ink/50">Work sets</span>
                <span class="font-display text-xl font-bold text-iron">{{ workWeights[lift.name] }}kg</span>
              </li>
            </ol>
          </div>
        </li>
      </template>
    </ul>
  </div>
</template>
