<template>
  <main class="container">
    <hgroup>
      <h1>Settings</h1>
      <p>Personal preferences.</p>
    </hgroup>

    <article>
      <header><strong>Weight Unit</strong></header>
      <fieldset>
        <label>
          <input type="radio" v-model="unit" value="kg" />
          Kilograms (kg)
        </label>
        <label>
          <input type="radio" v-model="unit" value="lbs" />
          Pounds (lbs)
        </label>
      </fieldset>
    </article>

    <article>
      <header><strong>Custom Plate Inventory</strong></header>
      <p class="secondary">Enter the plate sizes available in your gym (kg), comma-separated.</p>
      <label>
        Plate sizes
        <input v-model="plateInventory" placeholder="25, 20, 15, 10, 5, 2.5, 1.25" />
      </label>
      <small>These will be used in the plate calculator.</small>
    </article>

    <button @click="save">Save Settings</button>
    <p v-if="saved" style="color: var(--pico-ins-color);">Settings saved.</p>
  </main>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const unit = ref('kg')
const plateInventory = ref('25, 20, 15, 10, 5, 2.5, 1.25')
const saved = ref(false)

onMounted(() => {
  unit.value = localStorage.getItem('weightUnit') ?? 'kg'
  plateInventory.value = localStorage.getItem('plateInventory') ?? '25, 20, 15, 10, 5, 2.5, 1.25'
})

function save() {
  if (import.meta.client) {
    localStorage.setItem('weightUnit', unit.value)
    localStorage.setItem('plateInventory', plateInventory.value)
    saved.value = true
    setTimeout(() => { saved.value = false }, 2000)
  }
}
</script>
