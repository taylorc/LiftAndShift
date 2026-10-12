<script setup lang="ts">
defineProps<{ submitting: boolean; error: string | null }>()
const emit = defineEmits<{ submit: [payload: { recordedOn: string; bodyWeightKg: number }] }>()

const recordedOn = ref(todayIsoDate())
const bodyWeightKg = ref<number | null>(null)

function submit() {
  if (bodyWeightKg.value === null) return
  emit('submit', { recordedOn: recordedOn.value, bodyWeightKg: bodyWeightKg.value })
  bodyWeightKg.value = null
}
</script>

<template>
  <div>
    <form class="mt-8 flex flex-col gap-4 sm:flex-row sm:items-end" @submit.prevent="submit">
      <label class="flex flex-col gap-1">
        <span class="text-sm font-medium text-ink/70">Date</span>
        <input
          v-model="recordedOn"
          type="date"
          required
          class="border border-ink bg-paper px-3 py-2 focus:outline-2 focus:outline-iron"
        />
      </label>

      <label class="flex flex-1 flex-col gap-1">
        <span class="text-sm font-medium text-ink/70">Bodyweight (kg)</span>
        <input
          v-model.number="bodyWeightKg"
          type="number"
          inputmode="decimal"
          min="0.1"
          step="0.1"
          required
          class="border border-ink bg-paper px-3 py-2 focus:outline-2 focus:outline-iron"
        />
      </label>

      <button
        type="submit"
        :disabled="submitting"
        class="bg-iron px-6 py-2 font-medium text-paper transition-colors hover:bg-iron-dark disabled:opacity-50"
      >
        Log
      </button>
    </form>

    <p v-if="error" class="mt-3 text-sm text-terracotta">{{ error }}</p>
  </div>
</template>
