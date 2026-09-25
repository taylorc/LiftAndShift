<script setup lang="ts">
interface PersonalRecord {
  lift: string
  weightKg: number
  achievedOn: string
  workoutSessionId: number
}

const identity = useIdentityStore()
const api = useApi()

if (!identity.familyMemberId) {
  await navigateTo('/')
}

const { data: records, status } = await useAsyncData('personal-records', () =>
  api<PersonalRecord[]>(`/FamilyMembers/${identity.familyMemberId}/PersonalRecords`)
)

const sortedRecords = computed(() => [...(records.value ?? [])].sort((a, b) => b.weightKg - a.weightKg))

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })
}
</script>

<template>
  <div class="max-w-xl">
    <h1 class="font-display text-5xl font-black tracking-tight">Personal Records</h1>
    <p class="mt-1 text-sm text-ink/60">
      Training as {{ identity.name }}. The heaviest you've ever hit every set on, per lift.
    </p>

    <ul v-if="sortedRecords.length > 0" class="mt-8 border-t border-chalk">
      <li
        v-for="record in sortedRecords"
        :key="record.lift"
        class="flex items-center justify-between border-b border-l-4 border-chalk border-l-iron py-4 pl-4"
      >
        <div>
          <span class="font-display text-2xl font-bold">{{ formatLiftName(record.lift) }}</span>
          <p class="text-sm text-ink/50">{{ formatDate(record.achievedOn) }}</p>
        </div>
        <span class="font-display text-4xl font-black text-iron">{{ record.weightKg }}kg</span>
      </li>
    </ul>

    <p v-else-if="status === 'success'" class="mt-8 border-t border-chalk pt-6 text-ink/60">
      No records yet. Log a session where every set hits its target to set your first one.
    </p>
  </div>
</template>
