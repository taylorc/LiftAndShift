# Glossary / Ubiquitous Language

Terms used consistently across the domain model, code, and product spec (`StrartingStrength-UserStories.md`).

| Term | Meaning |
|---|---|
| **Programme** | A named training template (e.g. "Novice Phase 1", "Novice Phase 2") a lifter adopts. Represented by `UserProgramme.ProgrammeTemplateId` — templates themselves are not yet a domain entity (see [domain-model.md](domain-model.md)); they're returned by `GetProgrammeTemplatesQuery`. |
| **Phase** | A variant of the Novice programme. **Phase 1** trains Deadlift every session. **Phase 2** alternates Deadlift with the lifter's chosen accessory lift (`AlternatingLiftType`) each session. |
| **Workout A / Workout B** | The two alternating session templates in the 3-day/week rotation (`WorkoutType`). Workout A: Squat 3x5, Bench Press 3x5, Deadlift 1x5. Workout B: Squat 3x5, Overhead Press 3x5, Power Clean 5x3 (or the alternating lift). |
| **Alternating lift** | The Phase 2 accessory chosen at onboarding: Power Clean or Pendlay Row (`AlternatingLiftType`). |
| **Work set** / **Working set** | A set performed at the programmed training weight, as opposed to a warm-up (`SetType.WorkingSet`). |
| **Warm-up set** | A lighter preparatory set generated from the work set weight (empty bar, 40%, 60%, 80% chunks per US5), `SetType.Warmup`. |
| **Linear progression** | The core coaching algorithm: increase a lift's weight by a fixed increment every time all programmed sets/reps are completed successfully. Implemented in `StartingStrengthProgressionService`. |
| **Increment** | The fixed weight added to a lift on a successful session — 5 kg for Squat/Deadlift, 2.5 kg for Bench/OHP/Power Clean (rounded to the nearest 1.25 kg). Product spec (US6) states these in lb (10 lb / 5 lb); code stores kg as canonical. |
| **Consecutive failure** | A session where a lift's programmed reps were not all completed. The programme holds weight steady for the first two consecutive failures on a lift, then triggers a deload on the third. |
| **Deload** | A 10% reduction applied to a lift's working weight after 3 consecutive failures, giving the lifter a lighter reset session (`StartingStrengthProgressionService.ApplyDeload`). |
| **Programme session** | One scheduled or completed occurrence of the adopted programme (`ProgrammeSession`), distinct from... |
| **Workout session** | ...the actual logged gym session (`WorkoutSession`), which may or may not be tied 1:1 to a programme session (`IsProgrammeSession`). |
| **Lift progression** | Per-session snapshot of each lift's current working weight, stored as JSON on `ProgrammeSession.LiftProgression`. |
| **Estimated 1RM** | Estimated one-rep max, computed from a completed set's weight and reps; stored on `PersonalRecord.Estimated1RmKg` and charted per US13. |
| **Tonnage** | Total weight moved in a session (sum of weight × reps across completed sets) — a progress-tracking metric from US13; not currently a persisted/computed domain field, calculated at query time. |
| **Plate calculator** | Utility that breaks a target barbell weight down into the plates to load per side, given a 45 lb or 20 kg bar (`PlateCalculatorService`, US10). |
| **Rest timer** | Frontend countdown (default 3 min, up to 7 min) that auto-starts when a `WorkoutSet.IsCompleted` is checked off (US9). Not a backend concept. |
| **Weight unit** | The lifter's preferred display unit (`WeightUnit`: `Lbs` or `Kgs`). All storage is in kg; conversion is presentation-only. |
