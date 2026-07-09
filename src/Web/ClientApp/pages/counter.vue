<script setup lang="ts">
const count = ref(0)
</script>

<template>
  <div>
    <h1>Counter</h1>
    <p>This is a simple example of a Vue component.</p>
    <p aria-live="polite">Current count: <strong>{{ count }}</strong></p>
    <button @click="count++">Increment</button>
  </div>
</template>
