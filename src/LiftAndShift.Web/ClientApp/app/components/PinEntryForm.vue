<script setup lang="ts">
defineProps<{ memberName: string; error: string | null }>()
const emit = defineEmits<{ submit: [pin: string]; cancel: [] }>()

const pin = ref('')

function submit() {
  emit('submit', pin.value)
  pin.value = ''
}
</script>

<template>
  <div>
    <form class="flex items-end gap-3 pb-4 pl-4" @submit.prevent="submit">
      <label class="flex flex-col gap-1">
        <span class="text-sm font-medium text-ink/70">Enter {{ memberName }}'s PIN</span>
        <input
          v-model="pin"
          type="text"
          inputmode="numeric"
          pattern="[0-9]{4}"
          maxlength="4"
          required
          autofocus
          class="w-28 border border-ink bg-paper px-3 py-2 tracking-widest focus:outline-2 focus:outline-iron"
        />
      </label>
      <button type="submit" class="bg-iron px-6 py-2 font-medium text-paper transition-colors hover:bg-iron-dark">
        Continue
      </button>
      <button type="button" class="px-3 py-2 text-sm text-ink/60 hover:text-ink" @click="emit('cancel')">
        Cancel
      </button>
    </form>
    <p v-if="error" class="pb-4 pl-4 text-sm text-terracotta">{{ error }}</p>
  </div>
</template>
