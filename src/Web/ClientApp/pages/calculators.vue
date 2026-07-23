<template>
  <main class="container">
    <hgroup>
      <h1>Calculators</h1>
      <p>Plate loading and warmup tools.</p>
    </hgroup>

    <div style="display: flex; gap: 1rem; margin-bottom: 1rem;">
      <button :class="tab === 'plates' ? '' : 'outline'" @click="tab = 'plates'">Plate Calculator</button>
      <button :class="tab === 'warmup' ? '' : 'outline'" @click="tab = 'warmup'">Warmup Calculator</button>
    </div>

    <PlateCalculator v-if="tab === 'plates'" />
    <WarmupCalculator v-else />
  </main>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })
const tab = ref<'plates' | 'warmup'>('plates')
</script>
