<template>
  <main class="container">
    <hgroup>
      <h1>Programme</h1>
      <p>Your structured training plan.</p>
    </hgroup>

    <div v-if="loading" aria-busy="true">Loading...</div>

    <!-- Active programme -->
    <div v-else-if="store.activeProgramme">
      <article>
        <header style="display: flex; justify-content: space-between;">
          <strong>{{ store.activeProgramme.programmeName }}</strong>
          <small>Session {{ store.activeProgramme.sessionCount }}</small>
        </header>
        <p>Started {{ formatDate(store.activeProgramme.startedAt) }}</p>
      </article>

      <!-- Next session -->
      <section v-if="store.nextSession">
        <h2>Next Session — Workout {{ store.nextSession.workoutType }}</h2>
        <p class="secondary">Scheduled: {{ formatDate(store.nextSession.scheduledDate) }}</p>

        <article v-for="lift in store.nextSession.prescribedLifts" :key="lift.liftName">
          <header>
            <strong>{{ lift.liftName }}</strong>
            <span style="float: right;">{{ lift.sets }} × {{ lift.reps }} @ {{ lift.weightKg }} kg</span>
          </header>

          <details>
            <summary>Warmup sets ({{ lift.warmupSets.length }})</summary>
            <table role="grid">
              <thead><tr><th>#</th><th>Weight (kg)</th><th>Reps</th></tr></thead>
              <tbody>
                <tr v-for="ws in lift.warmupSets" :key="ws.setNumber">
                  <td>{{ ws.setNumber }}</td>
                  <td>{{ ws.weightKg }}</td>
                  <td>{{ ws.reps }}</td>
                </tr>
              </tbody>
            </table>
          </details>
        </article>

        <button @click="startSession" :aria-busy="starting">
          Start This Session
        </button>
      </section>

      <div v-else>
        <p class="secondary">All sessions completed. The next session will be generated after you log one.</p>
      </div>
    </div>

    <!-- No programme -->
    <div v-else>
      <p>You don't have an active programme. Choose one below:</p>
      <div v-if="templatesLoading" aria-busy="true">Loading templates...</div>
      <article v-for="tmpl in store.templates" :key="tmpl.id">
        <header><strong>{{ tmpl.name }}</strong></header>
        <p>{{ tmpl.description }}</p>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1rem;">
          <div>
            <strong>Workout A</strong>
            <ul style="margin: 0.25rem 0 0;">
              <li v-for="ex in tmpl.workoutAExercises" :key="ex">{{ ex }}</li>
            </ul>
          </div>
          <div>
            <strong>Workout B</strong>
            <ul style="margin: 0.25rem 0 0;">
              <li v-for="ex in tmpl.workoutBExercises" :key="ex">{{ ex }}</li>
            </ul>
          </div>
        </div>
        <button @click="adopt(tmpl.id)" :aria-busy="adopting === tmpl.id">
          Start {{ tmpl.name }}
        </button>
      </article>
    </div>
  </main>
</template>

<script setup lang="ts">
import { useProgrammeStore } from '~/stores/programme'

definePageMeta({ middleware: 'auth' })

const store = useProgrammeStore()
const loading = ref(true)
const templatesLoading = ref(false)
const adopting = ref<string | null>(null)
const starting = ref(false)
const router = useRouter()

onMounted(async () => {
  await store.fetchActiveProgramme()
  if (!store.activeProgramme) {
    templatesLoading.value = true
    await store.fetchTemplates()
    templatesLoading.value = false
  }
  loading.value = false
})

async function adopt(templateId: string) {
  adopting.value = templateId
  await store.adoptProgramme({ programmeTemplateId: templateId, startingWeights: {} })
  adopting.value = null
}

function startSession() {
  router.push('/log-workout')
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString()
}
</script>
