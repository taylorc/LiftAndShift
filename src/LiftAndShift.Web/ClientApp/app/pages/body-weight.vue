<script setup lang="ts">
import type { BodyMetric } from '~/types'

const identity = useIdentityStore()
const api = useApi()

if (!identity.familyMemberId) {
  await navigateTo('/')
}

const {
  data: entries,
  refresh,
  status
} = await useAsyncData('body-metrics', () =>
  api<{ items: BodyMetric[] }>(`/FamilyMembers/${identity.familyMemberId}/BodyMetrics`)
)

const logError = ref<string | null>(null)
const logging = ref(false)

async function logBodyWeight(payload: { recordedOn: string; bodyWeightKg: number }) {
  logError.value = null
  logging.value = true
  try {
    await api(`/FamilyMembers/${identity.familyMemberId}/BodyMetrics`, {
      method: 'POST',
      body: payload
    })
    await refresh()
  } catch {
    logError.value = 'Could not log that entry. Check the weight and try again.'
  } finally {
    logging.value = false
  }
}
</script>

<template>
  <div class="max-w-xl">
    <h1 class="font-display text-5xl font-black tracking-tight">Bodyweight</h1>
    <p class="mt-1 text-sm text-ink/60">
      Training as {{ identity.name }}. Step on the scale, write it down.
    </p>

    <BodyWeightForm :submitting="logging" :error="logError" @submit="logBodyWeight" />

    <ul v-if="(entries?.items.length ?? 0) > 0" class="mt-10 border-t border-chalk">
      <li
        v-for="entry in entries?.items"
        :key="entry.id"
        class="flex items-center justify-between border-b border-l-4 border-chalk border-l-iron py-4 pl-4"
      >
        <span class="text-ink/70">{{ formatDate(entry.recordedOn) }}</span>
        <span class="font-display text-3xl font-black text-iron">{{ entry.bodyWeightKg }}kg</span>
      </li>
    </ul>

    <p v-else-if="status === 'success'" class="mt-10 border-t border-chalk pt-6 text-ink/60">
      No entries yet. Log your first weigh-in above.
    </p>
  </div>
</template>
