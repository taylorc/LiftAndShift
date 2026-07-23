<template>
  <main class="container">
    <hgroup>
      <h1>Workout History</h1>
      <p>Your past training sessions.</p>
    </hgroup>

    <div v-if="loading" aria-busy="true">Loading...</div>

    <p v-else-if="!store.history.length" class="secondary">
      No workouts yet. <NuxtLink to="/log-workout">Log your first workout</NuxtLink>.
    </p>

    <div v-else>
      <article v-for="session in store.history" :key="session.id" style="margin-bottom: 1rem;">
        <header style="display: flex; justify-content: space-between; align-items: center;">
          <div>
            <strong>{{ formatDate(session.date) }}</strong>
            <small v-if="session.isProgrammeSession" style="margin-left: 0.5rem; color: var(--pico-primary);">Programme</small>
          </div>
          <span :class="session.status === 'Completed' ? 'tag-complete' : 'tag-draft'" class="status-tag">
            {{ session.status }}
          </span>
        </header>
        <p style="margin: 0.25rem 0; color: var(--pico-muted-color);">
          {{ session.exerciseNames.join(' · ') }}
        </p>
        <footer style="display: flex; justify-content: space-between; align-items: center;">
          <small>Volume: {{ session.totalVolumeKg.toFixed(1) }} kg</small>
          <NuxtLink :to="'/workout/' + session.id" role="button" class="outline" style="padding: 0.25rem 0.75rem; margin: 0;">
            View
          </NuxtLink>
        </footer>
      </article>
    </div>
  </main>
</template>

<script setup lang="ts">
import { useWorkoutStore } from '~/stores/workout'

definePageMeta({ middleware: 'auth' })

const store = useWorkoutStore()
const loading = ref(true)

onMounted(async () => {
  await store.fetchHistory()
  loading.value = false
})

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleString()
}
</script>

<style scoped>
.status-tag { padding: 0.2rem 0.5rem; border-radius: 99px; font-size: 0.75rem; }
.tag-complete { background: var(--pico-ins-color); color: #fff; }
.tag-draft { background: var(--pico-muted-border-color); }
</style>
