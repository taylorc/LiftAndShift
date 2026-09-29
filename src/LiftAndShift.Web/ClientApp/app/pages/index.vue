<script setup lang="ts">
import type { FamilyMember } from '~/types'

const api = useApi()
const identity = useIdentityStore()

const { data: familyMembers, refresh, status } = await useAsyncData('family-members', () =>
  api<{ items: FamilyMember[] }>('/FamilyMembers')
)

const addError = ref<string | null>(null)
const adding = ref(false)

async function addFamilyMember(payload: { name: string; pin: string }) {
  addError.value = null
  adding.value = true
  try {
    await api('/FamilyMembers', { method: 'POST', body: payload })
    await refresh()
  } catch {
    addError.value = 'Could not add family member. Check the name and PIN and try again.'
  } finally {
    adding.value = false
  }
}

const pendingMember = ref<FamilyMember | null>(null)
const pinError = ref<string | null>(null)

function startSelecting(member: FamilyMember) {
  pendingMember.value = member
  pinError.value = null
}

function cancelSelecting() {
  pendingMember.value = null
  pinError.value = null
}

function confirmSelection(pin: string) {
  if (!pendingMember.value) return

  if (pin === pendingMember.value.pin) {
    identity.select(pendingMember.value.id, pendingMember.value.name)
    cancelSelecting()
  } else {
    pinError.value = "That PIN doesn't match. Try again."
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

          <PinEntryForm
            v-if="pendingMember?.id === member.id"
            :member-name="member.name"
            :error="pinError"
            @submit="confirmSelection"
            @cancel="cancelSelecting"
          />
        </li>
      </template>

      <li v-if="status === 'success' && familyMembers?.items.length === 0" class="border-b border-chalk py-6 text-ink/60">
        No one's on the roster yet. Add the first family member below.
      </li>
    </ul>

    <AddFamilyMemberForm :submitting="adding" :error="addError" @submit="addFamilyMember" />
  </div>
</template>
