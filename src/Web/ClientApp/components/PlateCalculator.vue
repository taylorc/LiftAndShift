<template>
  <section>
    <form @submit.prevent="calculate">
      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
        <label>
          Target weight (kg)
          <input v-model.number="targetKg" type="number" step="0.5" min="0" required />
        </label>
        <label>
          Bar weight (kg)
          <input v-model.number="barKg" type="number" step="0.5" min="0" />
        </label>
      </div>
      <button type="submit">Calculate</button>
    </form>

    <div v-if="store.plateResult" style="margin-top: 1rem;">
      <p>
        <strong>Actual weight: {{ store.plateResult.actualWeightKg }} kg</strong>
        <span v-if="!store.plateResult.isExact" style="color: var(--pico-del-color);"> (nearest achievable)</span>
      </p>

      <!-- Visual bar diagram -->
      <div style="display: flex; align-items: center; gap: 0.5rem; margin: 1rem 0; flex-wrap: wrap;">
        <div v-for="[size, count] in platesFromSmallest" :key="size">
          <div v-for="n in count" :key="n" :style="plateStyle(Number(size))" class="plate-visual" id="leftPlates">
            {{ size }}
          </div>
        </div>
        <!-- Bar centre -->
        <div style="background: var(--pico-muted-border-color); width: 60px; height: 20px; display: flex; align-items: center; justify-content: center; font-size: 0.7rem;">
          BAR
        </div>
        <div v-for="[size, count] in platesFromLargest" :key="'r' + size">
          <div v-for="n in count" :key="n" :style="plateStyle(Number(size))" class="plate-visual" id="rightPlates">
            {{ size }}
          </div>
        </div>
      </div>

      <table role="grid">
        <thead>
          <tr><th>Plate (kg)</th><th>Per Side</th><th>Total Plates</th></tr>
        </thead>
        <tbody>
          <tr v-for="(count, size) in store.plateResult.platesPerSide" :key="size">
            <td>{{ size }}</td>
            <td>{{ count }}</td>
            <td>{{ count * 2 }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<script setup lang="ts">
import { useCalculatorStore } from '~/stores/calculator'

const store = useCalculatorStore()
const targetKg = ref(100)
const barKg = ref(20)

const platesFromLargest = computed(() => {
  if (!store.plateResult) return []
  return Object.entries(store.plateResult.platesPerSide ?? {}).sort(([a], [b]) => Number(b) - Number(a))
})

const platesFromSmallest = computed(() => {
  if (!store.plateResult) return []
  return Object.entries(store.plateResult.platesPerSide ?? {}).sort(([a], [b]) => Number(a) - Number(b))
})

function plateStyle(size: number) {
  const heights: Record<number, number> = { 25: 80, 20: 72, 15: 64, 10: 56, 5: 48, 2.5: 40, 1.25: 32 }
  const h = heights[size] ?? 40
  return {
    width: '30px',
    height: h + 'px',
    background: 'var(--pico-primary)',
    color: 'var(--pico-primary-inverse)',
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontSize: '0.65rem',
    borderRadius: '4px',
    margin: '0 2px',
  }
}

async function calculate() {
  await store.calculatePlates(targetKg.value, barKg.value)
}
</script>
