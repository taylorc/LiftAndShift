import { defineStore } from 'pinia'
import type { ExerciseDto, CreateExerciseCommand } from '~/lib/web-api-client'

export const useExerciseStore = defineStore('exercise', {
  state: () => ({
    exercises: [] as ExerciseDto[],
    loading: false,
    error: null as string | null,
  }),

  getters: {
    standardExercises: (state) => state.exercises.filter(e => !e.isCustom),
    customExercises: (state) => state.exercises.filter(e => e.isCustom),
    byMuscleGroup: (state) => (group: string) =>
      state.exercises.filter(e => e.muscleGroup === group),
  },

  actions: {
    async fetchExercises(search?: string, muscleGroup?: number, equipmentType?: number) {
      this.loading = true
      this.error = null
      try {
        const client = useExercisesClient()
        this.exercises = await client.getExercises(search, muscleGroup, equipmentType)
      } catch (e: any) {
        this.error = e?.message ?? 'Failed to load exercises'
      } finally {
        this.loading = false
      }
    },

    async createExercise(command: CreateExerciseCommand) {
      const client = useExercisesClient()
      const id = await client.createExercise(command)
      await this.fetchExercises()
      return id
    },

    async deleteExercise(id: number) {
      const client = useExercisesClient()
      await client.deleteExercise(id)
      this.exercises = this.exercises.filter(e => e.id !== id)
    },
  },
})
