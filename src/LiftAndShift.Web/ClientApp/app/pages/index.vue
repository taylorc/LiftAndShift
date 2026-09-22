<script setup lang="ts">
interface FamilyMember {
  id: number
  name: string
  pin: string
}

const api = useApi()

const { data: familyMembers, refresh, status } = await useAsyncData('family-members', () =>
  api<{ items: FamilyMember[] }>('/FamilyMembers')
)

const name = ref('')
const pin = ref('')
const error = ref<string | null>(null)
const submitting = ref(false)

async function addFamilyMember() {
  error.value = null
  submitting.value = true
  try {
    await api('/FamilyMembers', {
      method: 'POST',
      body: { name: name.value, pin: pin.value }
    })
    name.value = ''
    pin.value = ''
    await refresh()
  } catch {
    error.value = 'Could not add family member. Check the name and PIN and try again.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="max-w-xl">
    <h1 class="font-display text-5xl font-black tracking-tight">Family</h1>
    <p class="mt-1 text-sm text-ink/60">Everyone training on this bar.</p>

    <ul class="mt-8 border-t border-chalk">
      <li
        v-for="member in familyMembers?.items ?? []"
        :key="member.id"
        class="flex items-center justify-between border-b border-chalk py-4 pl-4 border-l-4 border-l-transparent transition-colors hover:border-l-iron"
      >
        <span class="font-display text-2xl font-bold">{{ member.name }}</span>
        <span class="text-sm tracking-widest text-ink/50">PIN &bull;&bull;&bull;&bull;</span>
      </li>

      <li v-if="status === 'success' && familyMembers?.items.length === 0" class="border-b border-chalk py-6 text-ink/60">
        No one's on the roster yet. Add the first family member below.
      </li>
    </ul>

    <form class="mt-10 flex flex-col gap-4 sm:flex-row sm:items-end" @submit.prevent="addFamilyMember">
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
