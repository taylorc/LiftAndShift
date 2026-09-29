<script setup lang="ts">
import type { Lift, WarmUpSet } from '~/types'

const props = defineProps<{ lift: Lift; workWeightKg: number | null }>()

const api = useApi()
const identity = useIdentityStore()

const sets = ref<WarmUpSet[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    sets.value = await api<WarmUpSet[]>(
      `/FamilyMembers/${identity.familyMemberId}/WorkWeights/${props.lift.name}/WarmUp`
    )
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="pb-6 pl-4 pr-2">
    <span class="text-sm font-medium tracking-widest text-ink/50">WARM-UP</span>
    <p v-if="loading" class="mt-2 text-ink/60">Loading…</p>
    <ol v-else class="mt-2 divide-y divide-chalk">
      <li v-for="(set, index) in sets" :key="index" class="flex items-baseline justify-between py-2">
        <span class="text-sm text-ink/50">Set {{ index + 1 }}</span>
        <span class="font-display text-xl font-bold"
          >{{ set.weightKg }}kg <span class="text-sm font-normal text-ink/50">× {{ set.reps }}</span></span
        >
      </li>
      <li class="flex items-baseline justify-between py-2">
        <span class="text-sm text-ink/50">Work sets</span>
        <span class="font-display text-xl font-bold text-iron">{{ workWeightKg }}kg</span>
      </li>
    </ol>
  </div>
</template>
