<script setup lang="ts">
interface Programme {
  id: number
  familyMemberId: number
  trainingPhase: number
}

const PHASE_LIFT: Record<number, string> = {
  1: 'Deadlift',
  2: 'Row',
  3: 'Lat Pulldown'
}

const identity = useIdentityStore()
const api = useApi()

if (!identity.familyMemberId) {
  await navigateTo('/')
}

const { data: programme, refresh, error: fetchError } = await useAsyncData(
  () => `programme-${identity.familyMemberId}`,
  () => api<Programme>(`/FamilyMembers/${identity.familyMemberId}/Programme`)
)

const advancing = ref(false)
const advanceError = ref<string | null>(null)

async function advancePhase() {
  advanceError.value = null
  advancing.value = true
  try {
    await api(`/FamilyMembers/${identity.familyMemberId}/Programme/AdvancePhase`, { method: 'POST' })
    await refresh()
  } catch {
    advanceError.value = "Couldn't advance your Training Phase. Try again."
  } finally {
    advancing.value = false
  }
}
</script>

<template>
  <div class="max-w-xl">
    <h1 class="font-display text-5xl font-black tracking-tight">Your Programme</h1>
    <p class="mt-1 text-sm text-ink/60">Training as {{ identity.name }}.</p>

    <p v-if="fetchError" class="mt-8 border-t border-chalk pt-6 text-terracotta">
      Couldn't load your Programme. Try refreshing.
    </p>

    <template v-else-if="programme">
      <div class="mt-8 border-t border-l-4 border-chalk border-l-iron py-6 pl-4">
        <span class="text-sm font-medium tracking-widest text-ink/50">TRAINING PHASE</span>
        <div class="font-display text-9xl font-black leading-none text-iron">{{ programme.trainingPhase }}</div>
        <p class="mt-2 text-ink/70">
          Workout B's third lift is
          <strong class="font-semibold text-ink">{{ PHASE_LIFT[programme.trainingPhase] }}</strong>.
        </p>
      </div>

      <div class="mt-6 border-t border-chalk pt-6">
        <p v-if="programme.trainingPhase >= 3" class="text-ink/70">
          You've reached the final Training Phase. There's nowhere further to advance.
        </p>
        <template v-else>
          <p class="text-sm text-ink/60">
            Only advance once you've judged yourself recovered from deadlifting — the app never decides this for you.
          </p>
          <button
            type="button"
            :disabled="advancing"
            class="mt-3 bg-iron px-6 py-2 font-medium text-paper transition-colors hover:bg-iron-dark disabled:opacity-50"
            @click="advancePhase"
          >
            Advance to Phase {{ programme.trainingPhase + 1 }}
          </button>
        </template>
        <p v-if="advanceError" class="mt-3 text-sm text-terracotta">{{ advanceError }}</p>
      </div>
    </template>
  </div>
</template>
