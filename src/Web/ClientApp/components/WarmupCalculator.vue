<template>
  <section>
    <form @submit.prevent="calculate">
      <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1rem;">
        <label>
          Working weight (kg)
          <input v-model.number="workingWeightKg" type="number" step="1.25" min="0" required />
        </label>
        <label>
          Bar weight (kg)
          <input v-model.number="barKg" type="number" step="0.5" min="0" />
        </label>
        <label>
          Steps
          <input v-model.number="steps" type="number" min="1" max="4" />
        </label>
      </div>
      <button type="submit">Calculate</button>
    </form>

    <table v-if="store.warmupSets.length" role="grid" style="margin-top: 1rem;">
      <thead>
        <tr>
          <th>Set</th>
          <th>Weight (kg)</th>
          <th>Reps</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="s in store.warmupSets" :key="s.setNumber">
          <td>{{ s.setNumber }}</td>
          <td>{{ s.weightKg }}</td>
          <td>{{ s.reps }}</td>
        </tr>
        <tr style="font-weight: bold; color: var(--pico-primary);">
          <td>Working</td>
          <td>{{ workingWeightKg }}</td>
          <td>5 × 3</td>
        </tr>
      </tbody>
    </table>
  </section>
</template>

<script setup lang="ts">
import { useCalculatorStore } from '~/stores/calculator'

const store = useCalculatorStore()
const workingWeightKg = ref(100)
const barKg = ref(20)
const steps = ref(4)

async function calculate() {
  await store.calculateWarmup(workingWeightKg.value, barKg.value, steps.value)
}
</script>
