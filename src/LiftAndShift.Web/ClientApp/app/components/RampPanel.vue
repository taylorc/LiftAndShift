<script setup lang="ts">
import type { Lift } from '~/types'

const props = defineProps<{ lift: Lift }>()
const emit = defineEmits<{ complete: []; cancel: [] }>()

const api = useApi()
const identity = useIdentityStore()

const currentSet = ref(1)
const error = ref<string | null>(null)
const submitting = ref(false)

const currentSetWeight = computed(
  () => props.lift.openingWeightKg + (currentSet.value - 1) * props.lift.incrementKg
)

function nextSet() {
  currentSet.value += 1
}

async function finish() {
  error.value = null
  submitting.value = true
  try {
    await api(`/FamilyMembers/${identity.familyMemberId}/WorkWeights/${props.lift.name}/CompleteRamp`, {
      method: 'POST',
      body: { finalSetNumber: currentSet.value }
    })
    emit('complete')
  } catch {
    error.value = "Couldn't record your Work Weight. Try again."
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="pb-6 pl-4 pr-2">
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
        @click="finish"
      >
        This is it — bar slowed
      </button>
      <button type="button" class="px-3 py-2 text-sm text-ink/60 hover:text-ink" @click="emit('cancel')">
        Cancel
      </button>
    </div>
    <p v-if="error" class="mt-3 text-sm text-terracotta">{{ error }}</p>
  </div>
</template>
