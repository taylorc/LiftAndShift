// Shared client-side DTOs mirroring the LiftAndShift API. Kept intentionally minimal - only the
// fields pages/components actually use - rather than a full mirror of the server-side models.

export type WorkoutName = 'A' | 'B'

// A Lift's name exactly as the API spells it (e.g. 'BenchPress'); use formatLiftName to display it.
export type LiftName = string

// Each lift's current Work Weight, or null for a lift that hasn't been Ramped yet.
export type WorkWeightsByLift = Record<LiftName, number | null>

export interface FamilyMember {
  id: number
  name: string
  pin: string
}

export interface Lift {
  id: number
  name: LiftName
  openingWeightKg: number
  incrementKg: number
  workSetCount: number
}

export interface Programme {
  id: number
  familyMemberId: number
  trainingPhase: number
  // Which lifts each Workout means at the current Training Phase, resolved server-side.
  workoutLifts: Record<WorkoutName, LiftName[]>
}

export interface WorkWeight {
  weightKg: number
}

export interface WarmUpSet {
  weightKg: number
  reps: number
}

export interface LiftOutcome {
  lift: LiftName
  successful: boolean
  newWeightKg: number
  deloaded: boolean
}

export interface WorkoutSession {
  liftOutcomes: LiftOutcome[]
}

export interface LoggedLiftInput {
  lift: LiftName
  weightKg: number
  repsPerSet: number[]
}

// What the Workout form collects; the page adds which Workout it was before POSTing.
export interface WorkoutSessionInput {
  performedOn: string
  loggedLifts: LoggedLiftInput[]
}

export interface PersonalRecord {
  lift: LiftName
  weightKg: number
  achievedOn: string
  workoutSessionId: number
}

export interface BodyMetric {
  id: number
  recordedOn: string
  bodyWeightKg: number
}
