# Domain Model

LiftAndShift's domain implements a Starting Strength-style novice barbell programme: onboarding a lifter's
starting weights, generating alternating A/B workouts, logging sets, and progressing weights linearly based on
success/failure. This document describes the entities, value objects, enums, and events in `src/Domain`, and how
they relate to each other.

## Entity relationship overview

```
UserProgramme 1───* ProgrammeSession ···> WorkoutSession (optional link, no FK enforced)
                                                │
WorkoutSession 1───* WorkoutExercise ──1─ Exercise
                          │
                          └──1───* WorkoutSet

PersonalRecord ──1─ Exercise

TodoList 1───* TodoItem

BodyMetric, ExerciseCategory — standalone (no navigation properties)
```

All entities inherit `BaseAuditableEntity` (→ `BaseEntity`), giving every row:

- `Id` (`int`)
- `Created` / `CreatedBy`
- `LastModified` / `LastModifiedBy`
- an in-memory `DomainEvents` list, drained and dispatched by a `SaveChanges` interceptor

## Entities

### Programme tracking

**UserProgramme** — a lifter's enrollment in a programme template.
| Property | Type | Notes |
|---|---|---|
| `UserId` | `string` | owner |
| `ProgrammeTemplateId` | `string` | which template was adopted (e.g. Novice Phase 1/2) |
| `StartedAt` | `DateTimeOffset` | |
| `Status` | `ProgrammeStatus` | `Active` (default), `Paused`, `Abandoned` |
| `SessionCount` | `int` | sessions completed so far |
| `CurrentWorkoutType` | `WorkoutType` | which of A/B is due next, default `A` |
| `Sessions` | `IList<ProgrammeSession>` | one-to-many, private setter |

**ProgrammeSession** — one scheduled/completed occurrence of the programme.
| Property | Type | Notes |
|---|---|---|
| `UserProgrammeId` / `UserProgramme` | `int` / nav | required parent |
| `WorkoutSessionId` | `int?` | loose link to the logged `WorkoutSession`, no FK/navigation |
| `WorkoutType` | `WorkoutType` | A or B |
| `ScheduledDate` | `DateTimeOffset` | |
| `CompletedDate` | `DateTimeOffset?` | null until logged |
| `LiftProgression` | `Dictionary<string, decimal>` | JSON column: lift name → current working weight, e.g. `{ "Squat": 100.0 }` — this is the linear-progression state machine's snapshot per session |

### Workout logging

**WorkoutSession** — an actual gym session (may or may not be tied to a programme).
| Property | Type | Notes |
|---|---|---|
| `UserId` | `string` | |
| `Date` | `DateTimeOffset` | |
| `Notes` | `string?` | |
| `Status` | `WorkoutStatus` | `Draft` (default) or `Completed` |
| `IsProgrammeSession` | `bool` | true if generated from a `UserProgramme` |
| `ProgrammeSessionId` | `int?` | loose back-link, no navigation |
| `Exercises` | `IList<WorkoutExercise>` | one-to-many, private setter |

**WorkoutExercise** — one exercise's slot within a session.
| Property | Type | Notes |
|---|---|---|
| `WorkoutSessionId` / `WorkoutSession` | `int` / nav | required parent |
| `ExerciseId` / `Exercise` | `int` / nav | required, which lift |
| `OrderIndex` | `int` | display/execution order |
| `Notes` | `string?` | |
| `Sets` | `IList<WorkoutSet>` | one-to-many, private setter |

**WorkoutSet** — a single set (warm-up or work set) of an exercise.
| Property | Type | Notes |
|---|---|---|
| `WorkoutExerciseId` / `WorkoutExercise` | `int` / nav | required parent |
| `SetNumber` | `int` | |
| `SetType` | `SetType` | `Warmup`, `WorkingSet` (default), `DropSet`, `AMRAP` |
| `WeightKg` | `decimal` | canonical storage unit is kg; lbs is a display concern (`WeightUnit`) |
| `Reps` | `int` | target reps |
| `CompletedReps` | `int?` | actual reps achieved, null until checked off |
| `Notes` | `string?` | |
| `IsCompleted` | `bool` | drives rest-timer trigger and success/failure evaluation |

### Exercises & records

**Exercise** — a liftable movement (seeded "Big 5" plus any user-created custom exercises).
| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | |
| `Description` | `string?` | |
| `MuscleGroup` | `MuscleGroup` | |
| `EquipmentType` | `EquipmentType` | |
| `MovementPattern` | `MovementPattern` | |
| `IsCustom` | `bool` | false for seeded programme lifts |
| `CreatedByUserId` | `string?` | set when `IsCustom` |
| `IsActive` | `bool` | soft-delete flag, default true |

**ExerciseCategory** — minimal lookup/grouping entity (`Name` only); not yet wired to `Exercise` via a navigation
property.

**PersonalRecord** — a lifter's best-known lift.
| Property | Type | Notes |
|---|---|---|
| `UserId` | `string` | |
| `ExerciseId` / `Exercise` | `int` / nav | required |
| `WeightKg` | `decimal` | |
| `Reps` | `int` | |
| `AchievedAt` | `DateTimeOffset` | |
| `Estimated1RmKg` | `decimal` | computed estimate at time of achievement |

### Body metrics

**BodyMetric** — a bodyweight log entry.
| Property | Type | Notes |
|---|---|---|
| `UserId` | `string` | |
| `Date` | `DateTimeOffset` | |
| `WeightKg` | `decimal` | |
| `Notes` | `string?` | |

### Carried-over template entities (not lifting-specific)

**TodoList** / **TodoItem** — retained from the Clean Architecture template as a reference vertical slice; not part
of the lifting domain. `TodoList 1───* TodoItem`; completing a `TodoItem` raises `TodoItemCompletedEvent`.

## Enums

| Enum | Values |
|---|---|
| `ProgrammeStatus` | `Active`, `Paused`, `Abandoned` |
| `WorkoutType` | `A`, `B` |
| `WorkoutStatus` | `Draft`, `Completed` |
| `SetType` | `Warmup`, `WorkingSet`, `DropSet`, `AMRAP` |
| `MuscleGroup` | `Legs`, `Back`, `Chest`, `Shoulders`, `Arms`, `Core`, `Full` |
| `EquipmentType` | `Barbell`, `Dumbbell`, `Bodyweight`, `Machine`, `Cable`, `Kettlebell` |
| `MovementPattern` | `Squat`, `Hinge`, `Push`, `Pull`, `Carry` |
| `PriorityLevel` | `None`, `Low`, `Medium`, `High` (Todo template) |

Two enums are modeled as [Ardalis SmartEnum](https://github.com/ardalis/SmartEnum) rather than plain C# enums,
because they carry lift-specific programme logic beyond a bare integer tag:

| SmartEnum | Values | Purpose |
|---|---|---|
| `WeightUnit` | `Lbs`, `Kgs` | user's display unit preference; storage stays in kg |
| `AlternatingLiftType` | `PowerClean`, `PendlayRow` | which accessory lift a lifter chose during onboarding to alternate with Deadlift in Programme Phase 2 |

## Value objects

**Colour** (`src/Domain/ValueObjects/Colour.cs`) — carried over from the template for `TodoList.Colour`. Wraps a
hex `Code` string, validated against a fixed palette (`Red`, `Orange`, `Green`, `Teal`, `Blue`, `Purple`, `Grey`).
Not used by the lifting domain.

## Domain events

**TodoItemCompletedEvent** — raised when `TodoItem.Done` transitions `false → true`; handled by
`Application/TodoItems/EventHandlers/LogTodoItemCompleted`.

No lifting-domain events exist yet (e.g. nothing fires today when a `WorkoutSet` is completed, a `WorkoutSession`
is completed, or linear progression triggers a deload). See [use-cases.md](use-cases.md) for where that logic
currently lives instead (in `StartingStrengthProgressionService` and the command handlers).

## Modeling notes / gaps

- `ProgrammeSession.WorkoutSessionId` and `WorkoutSession.ProgrammeSessionId` are both plain `int?` with no EF
  navigation property or enforced FK in either direction — the link between a scheduled programme session and the
  logged workout is currently soft.
- `ExerciseCategory` exists but has no relationship back to `Exercise`.
- Weight is stored in kg everywhere (`WeightKg`); lb/kg is purely a presentation choice via `WeightUnit`.
- Deload/failure history is **not persisted** anywhere in the domain. `StartingStrengthProgressionService.NextWeight`
  (`src/Application/Calculators`) takes `consecutiveFailures` as a caller-supplied `int` and deloads at `>= 3`, but
  `LogProgrammeSessionCommand` receives that count as a `Dictionary<string, int> ConsecutiveFailures` on the
  *request* — i.e. the client is trusted to track and report each lift's failure streak. `ProgrammeSession.LiftProgression`
  only stores the resulting current weight per lift, not the failure count that produced it. This is a real gap for
  User Story 8 (Failure and Deload Logic): nothing server-side currently verifies or reconstructs the streak from
  history.
