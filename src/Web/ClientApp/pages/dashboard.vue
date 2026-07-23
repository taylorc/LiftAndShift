<template>
  <main class="container">
    <hgroup>
      <h1>Dashboard</h1>
      <p>Welcome back! Here's your training overview.</p>
    </hgroup>

    <div v-if="loading" aria-busy="true">Loading dashboard...</div>

    <div v-else-if="data">
      <!-- Stats row -->
      <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; margin-bottom: 1.5rem;">
        <article style="text-align: center; margin: 0;">
          <h2 style="margin: 0;">{{ data.sessionsThisWeek }}</h2>
          <p style="margin: 0; color: var(--pico-muted-color);">This week</p>
        </article>
        <article style="text-align: center; margin: 0;">
          <h2 style="margin: 0;">{{ data.sessionsThisMonth }}</h2>
          <p style="margin: 0; color: var(--pico-muted-color);">This month</p>
        </article>
        <article style="text-align: center; margin: 0;">
          <h2 style="margin: 0;">{{ data.currentStreak }}</h2>
          <p style="margin: 0; color: var(--pico-muted-color);">Day streak</p>
        </article>
      </div>

      <!-- Next session card -->
      <article v-if="data.hasActiveProgramme && data.nextSessionDate">
        <header>
          <strong>Next Session — Workout {{ data.nextWorkoutType }}</strong>
        </header>
        <p>Scheduled: {{ formatDate(data.nextSessionDate) }}</p>
        <NuxtLink to="/programme" role="button">Go to Programme</NuxtLink>
      </article>
      <article v-else-if="!data.hasActiveProgramme">
        <header><strong>No active programme</strong></header>
        <p>Adopt a programme to get prescribed workouts with automatic weight progression.</p>
        <NuxtLink to="/programme" role="button" class="outline">Browse Programmes</NuxtLink>
      </article>

      <!-- PRs -->
      <section>
        <h2>Personal Records</h2>
        <PrTable :records="data.personalRecords" />
      </section>
    </div>
  </main>
</template>

<script setup lang="ts">
import type { DashboardDto } from '~/lib/web-api-client'

definePageMeta({ middleware: 'auth' })

const data = ref<DashboardDto | null>(null)
const loading = ref(true)

onMounted(async () => {
  try {
    const client = useDashboardClient()
    data.value = await client.getDashboard()
  } finally {
    loading.value = false
  }
})

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString()
}
</script>
