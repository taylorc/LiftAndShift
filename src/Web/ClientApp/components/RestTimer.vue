<template>
  <div style="display: flex; align-items: center; gap: 1rem; background: var(--pico-card-background-color); padding: 0.75rem 1rem; border-radius: var(--pico-border-radius);">
    <Timer :size="20" />
    <span>Rest: <strong>{{ formatted }}</strong></span>
    <button type="button" class="outline secondary" style="padding: 0.25rem 0.75rem; margin: 0;" @click="reset">Reset</button>
    <button type="button" class="outline contrast" style="padding: 0.25rem 0.75rem; margin: 0;" @click="$emit('done')">Done</button>
  </div>
</template>

<script setup lang="ts">
import { Timer } from '@lucide/vue'

const props = withDefaults(defineProps<{ seconds?: number }>(), { seconds: 180 })
const emit = defineEmits<{ (e: 'done'): void }>()

const remaining = ref(props.seconds)
let interval: ReturnType<typeof setInterval> | null = null

const formatted = computed(() => {
  const m = Math.floor(remaining.value / 60)
  const s = remaining.value % 60
  return `${m}:${s.toString().padStart(2, '0')}`
})

function start() {
  interval = setInterval(() => {
    if (remaining.value <= 0) {
      clearInterval(interval!)
      emit('done')
    } else {
      remaining.value--
    }
  }, 1000)
}

function reset() {
  if (interval) clearInterval(interval)
  remaining.value = props.seconds
  start()
}

onMounted(() => start())
onUnmounted(() => { if (interval) clearInterval(interval) })
</script>
