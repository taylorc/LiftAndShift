# Application Use Cases

Maps the product's user stories (`StrartingStrength-UserStories.md`) to the Application-layer vertical slices
(`src/Application/<Feature>/{Commands,Queries}`) and Web API endpoints (`src/Web/Endpoints`) that implement them.
Each `Application` feature is a MediatR command/query; each `Web` group exposes it over HTTP.

## Epic 1 — Account Setup & Onboarding

| Story | Feature slice | Command/Query | Endpoint |
|---|---|---|---|
| US1 Authentication | Users (ASP.NET Identity, not a custom slice) | — | `POST /api/users/logout`, plus `MapIdentityApi<ApplicationUser>()` defaults (register/login/confirm-email/etc.) |
| US2 Onboarding Metrics (unit, bodyweight, alternating lift choice, starting weights) | `Onboarding` | `SaveUserOnboardingCommand`, `GetUserOnboardingQuery` | `GET /api/onboarding`, `POST /api/onboarding` |
| US3 Programme Phase Selection | `Programmes` | `AdoptProgrammeCommand`, `GetProgrammeTemplatesQuery` | `GET /api/programmes/templates`, `POST /api/programmes/adopt` |

## Epic 2 — Workout Generation & Execution

| Story | Feature slice | Command/Query | Endpoint |
|---|---|---|---|
| US4 Automated Workout Rotation (A/B, 3-day cadence) | `Programmes` | `AdoptProgrammeCommand` seeds `UserProgramme.CurrentWorkoutType`; `LogProgrammeSessionCommand` advances it and produces the next `ProgrammeSession` | `GET /api/programmes/active`, `POST /api/programmes/{id}/log-session` |
| US4 (logging the actual session) | `Workouts` | `LogWorkoutCommand`, `CompleteWorkoutCommand`, `DuplicateWorkoutCommand` | `POST /api/workouts`, `POST /api/workouts/{id}/complete`, `POST /api/workouts/{id}/duplicate` |
| US5 Warm-up Set Generation | `Calculators` | `WarmupCalculatorService` (plain service, not CQRS) | `GET /api/calculators/warmup` |

## Epic 3 — Linear Progression Engine

| Story | Feature slice | Command/Query | Endpoint |
|---|---|---|---|
| US6 Automated Weight Increments | `Calculators` → `StartingStrengthProgressionService.ApplyIncrement` (Squat/Deadlift +5kg, others +2.5kg, rounded to nearest 1.25kg), invoked from `LogProgrammeSessionCommand` | — | `POST /api/programmes/{id}/log-session` |
| US7 Manual Increment Adjustments | *Not yet implemented* — `StartingStrengthProgressionService` hardcodes increments per lift name; no per-user override exists in `Onboarding` or elsewhere | — | — |
| US8 Failure and Deload Logic | `Calculators` → `StartingStrengthProgressionService.ShouldDeload` / `ApplyDeload` (10% reduction at 3 consecutive failures) | — | `POST /api/programmes/{id}/log-session` (`ConsecutiveFailures` supplied by the caller — see [domain-model.md](domain-model.md#modeling-notes--gaps) for the gap this implies) |

## Epic 4 — In-Gym Utilities & UX

| Story | Feature slice | Command/Query | Endpoint |
|---|---|---|---|
| US9 Automated Rest Timer | Frontend-only (Nuxt/Pinia); starts on `WorkoutSet.IsCompleted` checkoff | — | — |
| US10 Barbell Plate Calculator | `Calculators` | `PlateCalculatorService` | `GET /api/calculators/plates` |
| US11 High-Contrast & Accessible UI | Frontend-only | — | — |
| US12 Offline Capability & Auto-Sync | Frontend-only (PWA/Service Worker) | — | — |

## Epic 5 — Progress Analytics

| Story | Feature slice | Command/Query | Endpoint |
|---|---|---|---|
| US13 Progress Tracking Visualizations | `Workouts`, `BodyMetrics`, `Dashboard` | `GetExerciseProgressQuery`, `GetWorkoutHistoryQuery`, `GetBodyMetricsQuery`, `GetDashboardQuery` | `GET /api/workouts/progress/{exerciseId}`, `GET /api/workouts`, `GET /api/bodymetrics`, `GET /api/dashboard` |

## Supporting slices not tied to a user story

- **Exercises** — `CreateExerciseCommand`, `DeleteExerciseCommand`, `GetExercisesQuery`: manage the exercise
  library (seeded Big 5 lifts + custom user exercises). Backs the exercise picker used when logging workouts.
- **TodoLists / TodoItems / WeatherForecasts** — retained from the Clean Architecture template as reference
  vertical slices; not part of the lifting product.

## Cross-cutting pipeline

Every command/query passes through the MediatR pipeline (`Application/DependencyInjection.cs`), in order:
logging (pre-processor) → `UnhandledExceptionBehaviour` → `AuthorizationBehaviour` (`[Authorize]`) →
`ValidationBehaviour` (FluentValidation) → `PerformanceBehaviour`. This is where cross-story concerns like
"must be authenticated" and "must be a valid onboarding payload" are enforced, rather than in the handlers
themselves.
