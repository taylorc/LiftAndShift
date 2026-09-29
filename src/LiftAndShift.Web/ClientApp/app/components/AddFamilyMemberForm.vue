<script setup lang="ts">
defineProps<{ submitting: boolean; error: string | null }>()
const emit = defineEmits<{ submit: [payload: { name: string; pin: string }] }>()

const name = ref('')
const pin = ref('')

function submit() {
  emit('submit', { name: name.value, pin: pin.value })
  name.value = ''
  pin.value = ''
}
</script>

<template>
  <div>
    <form class="mt-10 flex flex-col gap-4 sm:flex-row sm:items-end" @submit.prevent="submit">
      <label class="flex flex-1 flex-col gap-1">
        <span class="text-sm font-medium text-ink/70">Name</span>
        <input
          v-model="name"
          type="text"
          required
          minlength="2"
          maxlength="100"
          class="border border-ink bg-paper px-3 py-2 focus:outline-2 focus:outline-iron"
        />
      </label>

      <label class="flex flex-col gap-1">
        <span class="text-sm font-medium text-ink/70">4-digit PIN</span>
        <input
          v-model="pin"
          type="text"
          inputmode="numeric"
          pattern="[0-9]{4}"
          maxlength="4"
          required
          class="w-28 border border-ink bg-paper px-3 py-2 tracking-widest focus:outline-2 focus:outline-iron"
        />
      </label>

      <button
        type="submit"
        :disabled="submitting"
        class="bg-iron px-6 py-2 font-medium text-paper transition-colors hover:bg-iron-dark disabled:opacity-50"
      >
        Add
      </button>
    </form>

    <p v-if="error" class="mt-3 text-sm text-terracotta">{{ error }}</p>
  </div>
</template>
