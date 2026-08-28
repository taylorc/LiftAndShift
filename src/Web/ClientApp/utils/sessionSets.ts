type CheckableSet = {
  setType?: number
  completedReps?: number | null
  isCompleted?: boolean
}

/**
 * The gate for completing/saving a programme session: every working set (setType 1) must have
 * its completed reps entered, and any set — working or warm-up — that has completed reps entered
 * must be ticked Done. Mirrors LogProgrammeSessionCommandValidator / EditProgrammeSessionCommandValidator.
 */
export function sessionSetsReady(exercises: { sets: CheckableSet[] }[]): boolean {
  return exercises.every((ex) =>
    ex.sets.every((set) => {
      const repsEntered = set.completedReps !== null && set.completedReps !== undefined
      if (set.setType === 1 && !repsEntered) return false
      if (repsEntered && !set.isCompleted) return false
      return true
    }),
  )
}
