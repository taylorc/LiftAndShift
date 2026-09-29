// Shared client-side DTOs mirroring the LiftAndShift API. Kept intentionally minimal - only the
// fields pages/components actually use - rather than a full mirror of the server-side models.

export interface FamilyMember {
  id: number
  name: string
  pin: string
}

export interface Lift {
  id: number
  name: string
  openingWeightKg: number
  incrementKg: number
  workSetCount: number
}

export interface Programme {
  id: number
  familyMemberId: number
  trainingPhase: number
}

export interface WorkWeight {
  weightKg: number
}

export interface WarmUpSet {
  weightKg: number
  reps: number
}

export interface LiftOutcome {
  lift: string
  successful: boolean
  newWeightKg: number
  deloaded: boolean
}

export interface WorkoutSession {
  liftOutcomes: LiftOutcome[]
}

export interface PersonalRecord {
  lift: string
  weightKg: number
  achievedOn: string
  workoutSessionId: number
}
