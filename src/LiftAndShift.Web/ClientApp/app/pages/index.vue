<script setup lang="ts">
interface FamilyMember {
  id: number
  name: string
  pin: string
}

const api = useApi()
const identity = useIdentityStore()

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

const pendingMember = ref<FamilyMember | null>(null)
const pinEntry = ref('')
const pinError = ref<string | null>(null)

function startSelecting(member: FamilyMember) {
  pendingMember.value = member
  pinEntry.value = ''
  pinError.value = null
}

function cancelSelecting() {
  pendingMember.value = null
  pinEntry.value = ''
  pinError.value = null
}

function confirmSelection() {
  if (!pendingMember.value) return

  if (pinEntry.value === pendingMember.value.pin) {
    identity.select(pendingMember.value.id, pendingMember.value.name)
    cancelSelecting()
  } else {
    pinError.value = "That PIN doesn't match. Try again."
    pinEntry.value = ''
  }
}
</script>

<template>
  <div class="max-w-xl">
    <h1 class="font-display text-5xl font-black tracking-tight">Family</h1>
    <p class="mt-1 text-sm text-ink/60">Everyone training on this bar. Pick your name to switch to you.</p>

    <ul class="mt-8 border-t border-chalk">
      <template v-for="member in familyMembers?.items ?? []" :key="member.id">
        <li class="border-b border-chalk border-l-4" :class="identity.familyMemberId === member.id ? 'border-l-iron' : 'border-l-transparent'">
          <button
            type="button"
            class="flex w-full items-center justify-between py-4 pl-4 pr-2 text-left transition-colors hover:border-l-iron"
            :disabled="identity.familyMemberId === member.id"
            @click="startSelecting(member)"
          >
            <span class="font-display text-2xl font-bold">{{ member.name }}</span>
            <span class="text-sm tracking-widest text-ink/50">
              {{ identity.familyMemberId === member.id ? 'This is you' : 'PIN ••••' }}
            </span>
          </button>

          <form
            v-if="pendingMember?.id === member.id"
            class="flex items-end gap-3 pb-4 pl-4"
            @submit.prevent="confirmSelection"
          >
            <label class="flex flex-col gap-1">
              <span class="text-sm font-medium text-ink/70">Enter {{ member.name }}'s PIN</span>
              <input
                v-model="pinEntry"
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
            <button type="button" class="px-3 py-2 text-sm text-ink/60 hover:text-ink" @click="cancelSelecting">
              Cancel
            </button>
          </form>
          <p v-if="pendingMember?.id === member.id && pinError" class="pb-4 pl-4 text-sm text-terracotta">
            {{ pinError }}
          </p>
        </li>
      </template>

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
