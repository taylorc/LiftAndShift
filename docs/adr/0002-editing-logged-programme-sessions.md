---
status: proposed
---

# Editing logged programme sessions replays progression downstream

## Context

A `UserProgramme` holds an ordered list of `ProgrammeSession` rows. Each session carries two
JSON dictionaries — `LiftProgression` (working weight per lift) and `ConsecutiveFailures`
(failed-sessions-in-a-row per lift) — that are its **inputs**: the numbers the lifter is
prescribed when they start it. When a session is logged, `LogProgrammeSessionCommandHandler`
reads the current session's dictionaries, derives that session's per-lift outcome from the
submitted `WorkoutSet` data (`DetermineOutcome`), and writes a *new* session whose
dictionaries are `StartingStrengthProgressionService.NextWeight(...)` applied to the current
ones. Progression is therefore a fold: session _N+1_'s inputs are a pure function of session
_N_'s inputs plus session _N_'s logged working sets.

Today that fold only ever runs **forward, one step at a time, inline in the log handler**.
There is no way to:

- view a completed session's logged data (only `GetActiveProgramme`'s single *pending*
  session is exposed);
- correct a logged session (wrong weight typed, missed set recorded as complete);
- edit programme metadata (name, start date, status);
- delete a mis-logged session or the whole programme.

The user wants all four. The critical one is correcting logged set data: because progression
is a carry-forward fold, changing session _K_'s logged sets makes the stored inputs of
sessions _K+1 … N_ stale.

## Decision

**1. Extract the progression fold into a reusable recalculator.**
Move the weight/failure-count derivation out of `LogProgrammeSessionCommandHandler` into a
service — `ProgrammeProgressionRecalculator` — that takes a programme's sessions in order,
plus each logged session's working-set data, and rewrites every session's `LiftProgression`
and `ConsecutiveFailures` from a fixed **baseline** (session 1's inputs, which come from
`AdoptProgrammeCommand` and are never recomputed). The log handler calls it for the
append-one case; edit and delete call it for the replay case. One implementation of the
Starting Strength rules, exercised by every path that changes session state.

**2. Editing a logged session replays every later session.**
`EditProgrammeSessionCommand(programmeId, sessionId, exercises[])` overwrites the target
session's `WorkoutSession` set data in place, then runs the recalculator from `sessionId`'s
successor to the end of the chain, all in one `SaveChangesAsync`. The edited session's own
input dictionaries are unchanged (they were the prescription); only its outcome, and hence
everything downstream, moves.

**3. Deletion is restricted to the most recent logged session (first cut).**
`DeleteProgrammeSessionCommand` removes the latest logged `ProgrammeSession` + its
`WorkoutSession`, decrements `SessionCount`, and restores `CurrentWorkoutType` /
next-pending-session to what they were before that log. Deleting a *mid-chain* session would
also have to re-index the A/B alternation of every following session, which is out of scope
here. Deleting the whole `UserProgramme` cascades and is unrestricted.

**4. Metadata edits are chain-independent.**
`UpdateProgrammeCommand` covers name, `StartedAt`, and `Status`
(`Active | Paused | Abandoned` — no new status value). None of these feed progression, so no
replay.

**5. Direct edits to a session's prescribed weights are a manual override.**
Editing session _K_'s `LiftProgression` by hand (e.g. "the algorithm deloaded me but I want
to hold") is allowed via `UpdateProgrammeSessionInputsCommand`; the recalculator then treats
_K_'s edited values as the baseline for the _K → K+1_ step and replays forward. Last write
wins between this and a set-data edit on the same session.

## API surface

| Verb | Route | Command/Query | Replays? |
| --- | --- | --- | --- |
| GET | `/api/Programmes/{id}/sessions` | `GetProgrammeSessions` (completed sessions + logged data) | — |
| PUT | `/api/Programmes/{id}/sessions/{sid}` | `EditProgrammeSessionCommand` (set data) | from `sid`+1 |
| PATCH | `/api/Programmes/{id}/sessions/{sid}` | `UpdateProgrammeSessionInputsCommand` (prescribed weights) | from `sid`+1 |
| DELETE | `/api/Programmes/{id}/sessions/{sid}` | `DeleteProgrammeSessionCommand` (latest only) | truncate |
| PATCH | `/api/Programmes/{id}` | `UpdateProgrammeCommand` (name/date/status) | — |
| DELETE | `/api/Programmes/{id}` | `DeleteProgrammeCommand` | — |

`EditProgrammeSessionCommand` reuses `LogWorkoutExerciseDto` / `LogWorkoutSetDto` and the
validation rule added for session completion (working sets need `CompletedReps`; a set with
`CompletedReps` must be marked done).

## Considered options

- **Edit in place, no replay.** Persist the corrected sets, leave downstream weights as they
  were. Simplest, but every session after an edit then shows a prescription inconsistent with
  its own history — the tracker's one job (tell me next week's weight) becomes untrustworthy
  after any correction. Rejected.
- **Recompute lazily on read.** Don't store per-session dictionaries as durable state;
  derive the whole chain in `GetActiveProgramme`. Cleaner in theory, but it discards the
  existing schema, the audit trail of what was prescribed when, and makes every read O(chain
  length). Rejected for this change; revisit if the dictionaries become purely derived.
- **Full mid-chain delete with A/B re-indexing.** More capable, but the alternation rewrite
  and its edge cases (deleting a session that triggered a deload) are a feature of their own.
  Deferred.

## Consequences

- `LogProgrammeSessionCommandHandler` shrinks to: append session, then
  `recalculator.Replay(programme, fromSessionId: newSessionId)`. Its existing unit tests
  (`LogProgrammeSessionCommandHandlerTests`) become the characterisation tests for the
  extracted recalculator and must keep passing unchanged.
- New EF: no schema change for editing (set data and JSON columns already exist); confirm
  cascade-delete `UserProgramme → ProgrammeSession → WorkoutSession → WorkoutExercise →
  WorkoutSet` is configured, add it if not.
- Breaking API additions only (new endpoints); nswag client regen + a new Nuxt
  programme-history page that reuses `log-session.vue`'s set grid. Frontend is a later slice.
- `CONTEXT.md` gains **pending vs logged session** and **progression carry-forward**.
- Replay is bounded by chain length (tens of sessions); a whole-programme replay on every
  edit is acceptable and avoids "replay from K" off-by-one bugs — but the recalculator takes
  a `fromSessionId` so the log-append path stays O(1).

## Implementation slices (TDD)

1. Extract `ProgrammeProgressionRecalculator` behind the current inline fold; log handler
   delegates; existing handler tests stay green. **Seam:** the recalculator's public method.
2. `EditProgrammeSessionCommand` + validator + handler + endpoint. **Seam:** the command via
   the Mediator pipeline (functional test) asserting downstream sessions' dictionaries.
3. `GetProgrammeSessions` query + endpoint. **Seam:** the query handler.
4. `DeleteProgrammeSessionCommand` (latest-only guard) + `UpdateProgrammeCommand`. **Seams:**
   each command handler.
5. `UpdateProgrammeSessionInputsCommand`. **Seam:** command handler + recalculator baseline.
6. Nuxt: programme-history page, per-session edit reusing the set grid, metadata form.
   **Seam:** each page's rendered output.
