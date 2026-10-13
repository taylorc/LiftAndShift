import type { LiftName, WorkWeight, WorkWeightsByLift } from '~/types'

// Fetches the current Work Weight for each named lift, for the active Family Member. A lift that
// hasn't been Ramped yet has no Work Weight - the API 404s for it, which we treat as `null` rather
// than an error, since "not yet ramped" is an expected, common state.
export function useWorkWeights() {
  const api = useApi()
  const identity = useIdentityStore()

  async function fetchWorkWeights(liftNames: LiftName[]): Promise<WorkWeightsByLift> {
    const entries = await Promise.all(
      liftNames.map(async (name) => {
        try {
          const workWeight = await api<WorkWeight>(
            `/FamilyMembers/${identity.familyMemberId}/WorkWeights/${name}`
          )
          return [name, workWeight.weightKg] as const
        } catch {
          return [name, null] as const
        }
      })
    )
    return Object.fromEntries(entries)
  }

  return { fetchWorkWeights }
}
