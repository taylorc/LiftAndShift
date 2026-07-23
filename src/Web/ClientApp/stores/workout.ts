import { defineStore } from 'pinia'
import type {
  WorkoutHistoryItemDto,
  WorkoutDetailDto,
  LogWorkoutCommand,
  ExerciseProgressPointDto,
} from '~/lib/web-api-client'

export const useWorkoutStore = defineStore('workout', {
  state: () => ({
    history: [] as WorkoutHistoryItemDto[],
    currentWorkout: null as WorkoutDetailDto | null,
    progress: [] as ExerciseProgressPointDto[],
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async fetchHistory() {
      this.loading = true
      this.error = null
      try {
        const client = useWorkoutsClient()
        this.history = await client.getWorkoutHistory()
      } catch (e: any) {
        this.error = e?.message ?? 'Failed to load history'
      } finally {
        this.loading = false
      }
    },

    async fetchWorkout(id: number) {
      this.loading = true
      this.error = null
      try {
        const client = useWorkoutsClient()
        this.currentWorkout = await client.getWorkout(id)
      } catch (e: any) {
        this.error = e?.message ?? 'Failed to load workout'
      } finally {
        this.loading = false
      }
    },

    async logWorkout(command: LogWorkoutCommand): Promise<number> {
      const client = useWorkoutsClient()
      const id = await client.logWorkout(command)
      return id
    },

    async completeWorkout(id: number) {
      const client = useWorkoutsClient()
      await client.completeWorkout(id)
      if (this.currentWorkout?.id === id) {
        this.currentWorkout = { ...this.currentWorkout, status: 'Completed' }
      }
    },

    async duplicateWorkout(id: number): Promise<number> {
      const client = useWorkoutsClient()
      return await client.duplicateWorkout(id)
    },

    async fetchExerciseProgress(exerciseId: number) {
      this.loading = true
      this.error = null
      try {
        const client = useWorkoutsClient()
        this.progress = await client.getExerciseProgress(exerciseId)
      } catch (e: any) {
        this.error = e?.message ?? 'Failed to load progress'
      } finally {
        this.loading = false
      }
    },
  },
})
