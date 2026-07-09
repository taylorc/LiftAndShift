<script setup lang="ts">
import { Laptop, Sun, Moon } from '@lucide/vue'

const themeStore = useThemeStore()

const icons = { auto: Laptop, light: Sun, dark: Moon }
</script>

<template>
  <button
    class="theme-toggle-btn"
    :aria-label="themeStore.theme"
    @click="themeStore.cycleTheme()"
  >
    <component :is="icons[themeStore.theme]" :size="22" :stroke-width="2" />
  </button>
</template>
