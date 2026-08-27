# LiftAndShift

A Starting Strength-style linear-progression lifting tracker: onboarding, programme generation, workout logging, and automated weight progression.

## Language

**Working set**:
A set of type `WorkingSet` performed at the programmed training weight, as distinct from a warm-up.

**Lift success** (per session):
A trained lift's outcome for a session, when every evaluable working set (non-null `CompletedReps`) meets its programmed `Reps` target. A lift with no evaluable working sets logged that session is untrained, not a success or a failure.

**Lift failure** (per session):
A trained lift's outcome for a session, when at least one evaluable working set falls short of its programmed `Reps` target (`CompletedReps < Reps`). Null `CompletedReps` sets are excluded from evaluation, not counted as failures.

**Consecutive failures**:
A running per-lift counter of how many programme sessions in a row a lift has failed. Resets to 0 on success or deload; increments by 1 on failure. Derived server-side from logged set data — never trusted from the client.

**Deload**:
A 10% reduction applied to a lift's working weight, triggered when its consecutive failures reaches 3. Resets the lift's consecutive-failure counter to 0.

**Programme session**:
One scheduled A/B slot in a programme. **Pending** until the lifter logs a workout against it; **logged** once it has a completed workout and therefore a per-lift outcome. Only a logged session influences progression.

**Progression carry-forward**:
A lift's working weight and consecutive-failure count for a session are derived from the previous session's values plus that previous session's outcome, back to the programme's starting weights. Correcting a logged session's results therefore re-derives every later session's prescribed weights and failure counts.
