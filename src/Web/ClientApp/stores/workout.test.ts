import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'
import { setActivePinia, createPinia } from 'pinia'
import { useWorkoutStore } from './workout'
import type { LogWorkoutCommand } from '~/lib/web-api-client'

const {
  getWorkoutHistory,
  getWorkout,
  logWorkout,
  completeWorkout,
  duplicateWorkout,
  getExerciseProgress,
} = vi.hoisted(() => ({
  getWorkoutHistory: vi.fn(),
  getWorkout: vi.fn(),
  logWorkout: vi.fn(),
  completeWorkout: vi.fn(),
  duplicateWorkout: vi.fn(),
  getExerciseProgress: vi.fn(),
}))

mockNuxtImport('useWorkoutsClient', () => {
  return () => ({
    getWorkoutHistory,
    getWorkout,
    logWorkout,
    completeWorkout,
    duplicateWorkout,
    getExerciseProgress,
  })
})

describe('useWorkoutStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('fetchHistory', () => {
    it('stores the returned history and clears loading/error', async () => {
      const store = useWorkoutStore()
      getWorkoutHistory.mockResolvedValueOnce([{ id: 1 }, { id: 2 }])

      await store.fetchHistory()

      expect(store.history).toEqual([{ id: 1 }, { id: 2 }])
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('sets error and clears loading when the client rejects', async () => {
      const store = useWorkoutStore()
      getWorkoutHistory.mockRejectedValueOnce(new Error('boom'))

      await store.fetchHistory()

      expect(store.error).toBe('boom')
      expect(store.loading).toBe(false)
      expect(store.history).toEqual([])
    })

    it('falls back to a default error message when the rejection has none', async () => {
      const store = useWorkoutStore()
      getWorkoutHistory.mockRejectedValueOnce({})

      await store.fetchHistory()

      expect(store.error).toBe('Failed to load history')
    })
  })

  describe('fetchWorkout', () => {
    it('stores the returned workout and clears loading/error', async () => {
      const store = useWorkoutStore()
      getWorkout.mockResolvedValueOnce({ id: 5, status: 'InProgress' })

      await store.fetchWorkout(5)

      expect(getWorkout).toHaveBeenCalledWith(5)
      expect(store.currentWorkout).toEqual({ id: 5, status: 'InProgress' })
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('sets error and clears loading when the client rejects', async () => {
      const store = useWorkoutStore()
      getWorkout.mockRejectedValueOnce(new Error('not found'))

      await store.fetchWorkout(5)

      expect(store.error).toBe('not found')
      expect(store.loading).toBe(false)
      expect(store.currentWorkout).toBeNull()
    })
  })

  describe('logWorkout', () => {
    it('calls the client and returns the new workout id', async () => {
      const store = useWorkoutStore()
      logWorkout.mockResolvedValueOnce(42)

      const command = { isProgrammeSession: false } as LogWorkoutCommand
      const id = await store.logWorkout(command)

      expect(logWorkout).toHaveBeenCalledWith(command)
      expect(id).toBe(42)
    })
  })

  describe('completeWorkout', () => {
    it('marks the current workout Completed when its id matches', async () => {
      const store = useWorkoutStore()
      store.currentWorkout = { id: 5, status: 'InProgress' } as any
      completeWorkout.mockResolvedValueOnce(undefined)

      await store.completeWorkout(5)

      expect(completeWorkout).toHaveBeenCalledWith(5)
      expect(store.currentWorkout?.status).toBe('Completed')
    })

    it('leaves the current workout untouched when the id does not match', async () => {
      const store = useWorkoutStore()
      store.currentWorkout = { id: 5, status: 'InProgress' } as any
      completeWorkout.mockResolvedValueOnce(undefined)

      await store.completeWorkout(99)

      expect(store.currentWorkout).toEqual({ id: 5, status: 'InProgress' })
    })

    it('does nothing to currentWorkout when none is loaded', async () => {
      const store = useWorkoutStore()
      completeWorkout.mockResolvedValueOnce(undefined)

      await store.completeWorkout(5)

      expect(store.currentWorkout).toBeNull()
    })
  })

  describe('duplicateWorkout', () => {
    it('calls the client and returns the new workout id', async () => {
      const store = useWorkoutStore()
      duplicateWorkout.mockResolvedValueOnce(7)

      const id = await store.duplicateWorkout(5)

      expect(duplicateWorkout).toHaveBeenCalledWith(5)
      expect(id).toBe(7)
    })
  })

  describe('fetchExerciseProgress', () => {
    it('stores the returned progress points and clears loading/error', async () => {
      const store = useWorkoutStore()
      getExerciseProgress.mockResolvedValueOnce([{ date: '2026-01-01', weightKg: 100 }])

      await store.fetchExerciseProgress(9)

      expect(getExerciseProgress).toHaveBeenCalledWith(9)
      expect(store.progress).toEqual([{ date: '2026-01-01', weightKg: 100 }])
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('sets error and clears loading when the client rejects', async () => {
      const store = useWorkoutStore()
      getExerciseProgress.mockRejectedValueOnce(new Error('boom'))

      await store.fetchExerciseProgress(9)

      expect(store.error).toBe('boom')
      expect(store.loading).toBe(false)
      expect(store.progress).toEqual([])
    })
  })
})
