import { defineStore } from 'pinia'
import type { WarmupSet, PlateResult } from '~/lib/web-api-client'

export const useCalculatorStore = defineStore('calculator', {
  state: () => ({
    warmupSets: [] as WarmupSet[],
    plateResult: null as PlateResult | null,
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async calculateWarmup(weight: number, bar = 20, steps = 4) {
      this.loading = true
      this.error = null
      try {
        const client = useCalculatorsClient()
        this.warmupSets = await client.getWarmupSets(weight, bar, steps)
      } catch (e: any) {
        this.error = e?.message ?? 'Failed to calculate warmup'
      } finally {
        this.loading = false
      }
    },

    async calculatePlates(target: number, bar = 20, plates?: string) {
      this.loading = true
      this.error = null
      try {
        const client = useCalculatorsClient()
        this.plateResult = await client.getPlateCalculation(target, bar, plates)
      } catch (e: any) {
        this.error = e?.message ?? 'Failed to calculate plates'
      } finally {
        this.loading = false
      }
    },
  },
})
