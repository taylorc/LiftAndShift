# User Stories Document: Starting Strength Web Application

---

## Epic 1: Account Setup & Onboarding

### User Story 1: User Authentication
* **As a** new or returning user,
* **I want to** create an account or log in using Email, Google, or Apple ID,
* **So that** my personal training history is securely saved and protected.
    * **Acceptance Criteria:**
        * The interface must provide explicit signup and login options for Email, Google, and Apple ID.
        * All user credentials and profile data must be encrypted in transit via HTTPS and at rest.
        * The authentication system must comply with data privacy regulations such as GDPR and CCPA.

### User Story 2: Onboarding Metrics
* **As a** lifter setting up my profile,
* **I want to** input my preferred units, current body weight, and baseline working weights,
* **So that** the application can accurately calculate my target weights and progression steps.
    * **Acceptance Criteria:**
        * The application must allow the user to select either lbs or kgs as their preferred unit of measurement.
        * The system must capture and store the user's current body weight.
        * The onboarding flow must collect the initial "Starting Working Weights" for all 5 primary lifts: Squat, Bench Press, Overhead Press, Deadlift, and Power Clean.

### User Story 3: Program Phase Selection
* **As a** novice lifter configuring my program,
* **I want to** select my current Novice program phase,
* **So that** the app populates the correct exercise selections for my workouts.
    * **Acceptance Criteria:**
        * The application must allow the user to select their current tier of the novice program.
        * Option 1 must configure Phase 1, which utilizes the Deadlift only.
        * Option 2 must configure Phase 2, which features an alternating Deadlift and Power Clean schedule.

---

## Epic 2: Workout Generation & Execution

### User Story 4: Automated Workout Rotation
* **As a** lifter executing the program,
* **I want** the application to automatically alternate between Workout A and Workout B on a 3-day non-consecutive weekly schedule,
* **So that** I follow the exact cadence of the linear progression without managing spreadsheets.
    * **Acceptance Criteria:**
        * The system must auto-generate workouts on a 3-day non-consecutive weekly cadence (e.g., Monday, Wednesday, Friday).
        * The application must automatically alternate between Workout A and Workout B variations.
        * **Workout A** must generate the following prescription: Squat (3 sets x 5 reps), Bench Press (3 sets x 5 reps), and Deadlift (1 set x 5 reps).
        * **Workout B** must generate the following prescription: Squat (3 sets x 5 reps), Overhead Press (3 sets x 5 reps), and Power Clean (5 sets x 3 reps) or a designated Deadlift alternative.

### User Story 5: Warm-up Set Generation
* **As a** lifter preparing for an exercise,
* **I want** the system to automatically generate my warm-up sets based on my target work set weight,
* **So that** I know exactly how much weight to load for my warm-up increments without doing manual math.
    * **Acceptance Criteria:**
        * The system must calculate standard Starting Strength warm-up sets automatically using the day's target work set weight.
        * The generated warm-up structure must break down increments into an empty bar set, 40% chunk, 60% chunk, and 80% chunk.

---

## Epic 3: Linear Progression Engine

### User Story 6: Automated Weight Increments
* **As a** lifter who successfully completed all prescribed sets,
* **I want** the application to automatically increase my target weight for the next training session,
* **So that** I can maintain a strict linear progression.
    * **Acceptance Criteria:**
        * If a user successfully checks off all completed work sets and reps for a given lift, the target weight must automatically increase for the next session.
        * The default increment for the Squat and Deadlift must be set to +10 lbs / 5 kg.
        * The default increment for the Bench Press, Overhead Press, and Power Clean must be set to +5 lbs / 2.5 kg.

### User Story 7: Manual Increment Adjustments
* **As a** lifter utilizing custom plates or micro-loading,
* **I want to** override and manually adjust the default weight increments in my settings,
* **So that** I can progress at a customized pace when necessary.
    * **Acceptance Criteria:**
        * The application profile settings must include a configuration option to override default increment steps.
        * The system must accept custom manual values, specifically supporting values down to 1-lb plates for micro-loading.

### User Story 8: Failure and Deload Logic
* **As a** lifter who fails to hit my target repetitions,
* **I want** the app to manage my weight retention or recommend a deload,
* **So that** I can safely build back up and overcome training plateaus.
    * **Acceptance Criteria:**
        * If a user fails to hit the target reps (e.g., logging 5/4/3 instead of 5/5/5), the system must maintain the exact same target weight for the next workout during the first and second consecutive failures.
        * Upon registering a third consecutive failure on the same exercise, the application must trigger a Deload Recommendation.
        * The Deload Recommendation must automatically reduce the target weight by 10% for the next session to allow the user to build back up.

---

## Epic 4: In-Gym Utilities & User Experience

### User Story 9: Automated Rest Timer
* **As a** lifter resting between intensive work sets,
* **I want** an automated countdown timer to trigger as soon as I log a completed set,
* **So that** I rest for the appropriate amount of time before my next attempt.
    * **Acceptance Criteria:**
        * A countdown timer must trigger automatically immediately after a user checks off a completed set.
        * The timer must default to an initial countdown duration of 3 minutes.
        * The user must be able to customize the timer duration up to a maximum of 7 minutes.

### User Story 10: Barbell Plate Calculator
* **As a** lifter preparing to load the barbell,
* **I want** a visual breakdown of the plates needed for each side of the bar,
* **So that** I can load the target weight quickly and accurately without mental calculation errors.
    * **Acceptance Criteria:**
        * The application must provide a visual breakdown displaying exactly which plates to put on each side of the barbell to hit the day's target weight.
        * The system must support calculations based on a standard 45 lb or 20 kg barbell.

### User Story 11: High-Contrast & Accessible UI
* **As a** lifter exercising under harsh gym lighting,
* **I want** a high-contrast dark mode interface with large touch elements,
* **So that** I can easily read my log and tap inputs with sweaty or shaky hands.
    * **Acceptance Criteria:**
        * The interface must feature a high-contrast dark mode option designed specifically for gym environments.
        * Touch targets for checking off completed sets must be explicitly oversized to facilitate easy tapping under physical fatigue.

### User Story 12: Offline Capability & Auto-Sync
* **As a** lifter training in a gym with poor cellular data coverage,
* **I want to** log my workouts completely offline and have them save locally,
* **So that** I never lose my training data due to low connectivity.
    * **Acceptance Criteria:**
        * The web application must utilize local caching (Service Workers/PWA capabilities) to allow seamless offline workout logging.
        * Saved offline data must automatically sync back to the main server once an active internet connection is re-established.

---

## Epic 5: Progress Analytics

### User Story 13: Progress Tracking Visualizations
* **As a** lifter tracking my long-term training consistency,
* **I want to** view visual charts of my metric growth over time,
* **So that** I can analyze my linear progression and physical development.
    * **Acceptance Criteria:**
        * The application must display visual graphs tracking estimated 1-Rep Max (1RM) growth over time.
        * The application must display visual graphs tracking total tonnage lifted per session.
        * The application must display visual graphs tracking bodyweight changes over time.