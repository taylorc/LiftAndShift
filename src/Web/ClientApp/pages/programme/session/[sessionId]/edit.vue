<template>
  <main class="container">
    <hgroup>
      <h1>Edit session</h1>
      <p>Correcting logged sets re-derives every later session's prescribed weights.</p>
    </hgroup>

    <div v-if="loading" aria-busy="true">Loading...</div>

    <div v-else-if="!session">
      <p>That session was not found.</p>
      <NuxtLink to="/programme" role="button">Back to Programme</NuxtLink>
    </div>

    <form v-else @submit.prevent="save">
      <article v-for="(ex, idx) in exercises" :key="idx">
        <header><strong>{{ ex.exerciseName }}</strong></header>
        <SessionSetGrid :sets="ex.sets" />
      </article>

      <div style="display: flex; gap: 1rem;">
        <button type="submit" :aria-busy="saving" :disabled="!readyToSave">Save changes</button>
        <NuxtLink to="/programme" role="button" class="secondary">Cancel</NuxtLink>
      </div>

      <p v-if="!readyToSave" class="secondary">Enter completed reps for every working set, and tick Done for each set you've logged.</p>
      <p v-if="error" style="color: var(--pico-del-color);">{{ error }}</p>
    </form>
  </main>
</template>

<script setup lang="ts">
import { useProgrammeStore } from '~/stores/programme'
import { sessionSetsReady } from '~/utils/sessionSets'
import type { LogWorkoutSetDto } from '~/lib/web-api-client'

definePageMeta({ middleware: 'auth' })

const store = useProgrammeStore()
const route = useRoute()
const router = useRouter()

const sessionId = Number(route.params.sessionId)
const loading = ref(true)
const saving = ref(false)
const error = ref('')

const SET_TYPE: Record<string, number> = { Warmup: 0, WorkingSet: 1, DropSet: 2, AMRAP: 3 }

type EditableExercise = {
  exerciseId: number
  exerciseName: string
  orderIndex: number
  notes: string | null
  sets: LogWorkoutSetDto[]
}
const exercises = ref<EditableExercise[]>([])

const session = computed(() => store.sessions.find((s) => s.sessionId === sessionId) ?? null)
const readyToSave = computed(() => sessionSetsReady(exercises.value))

onMounted(async () => {
  if (!store.activeProgramme) {
    await store.fetchActiveProgramme()
  }
  if (store.activeProgramme) {
    await store.fetchProgrammeSessions(store.activeProgramme.id!)
  }

  if (session.value) {
    exercises.value = (session.value.exercises ?? []).map((ex) => ({
      exerciseId: ex.exerciseId!,
      exerciseName: ex.exerciseName ?? '',
      orderIndex: ex.orderIndex ?? 0,
      notes: ex.notes ?? null,
      sets: (ex.sets ?? []).map((s) => ({
        setNumber: s.setNumber,
        setType: SET_TYPE[s.setType ?? 'WorkingSet'] ?? 1,
        weightKg: s.weightKg,
        reps: s.reps,
        completedReps: s.completedReps ?? null,
        notes: s.notes ?? null,
        isCompleted: s.isCompleted,
      })) as LogWorkoutSetDto[],
    }))
  }

  loading.value = false
})

async function save() {
  error.value = ''
  saving.value = true
  try {
    await store.editProgrammeSession(store.activeProgramme!.id!, sessionId, {
      exercises: exercises.value.map(({ exerciseName, ...rest }) => rest),
    })
    router.push('/programme')
  } catch (e: any) {
    error.value = e?.message ?? 'Failed to save changes'
  } finally {
    saving.value = false
  }
}
</script>
