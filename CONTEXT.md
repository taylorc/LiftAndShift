# LiftAndShift

A family-hosted Starting Strength progression tracker. Each family member follows the same fixed novice barbell programme, logging sets and letting the app derive progression and deloads automatically.

## Language

**Family Member**:
A person who uses the app, identified by a PIN rather than a full account. Owns their own Programme, workout history, body metrics, and personal records.
_Avoid_: User, account, member (bare)

**PIN**:
A short code a Family Member enters to select their own identity on a shared device. Not authentication — it exists to prevent accidentally acting as someone else, not to secure data. Once entered, the selected identity persists on that device until someone switches to a different Family Member.
_Avoid_: Password, login, credentials

**Programme**:
The single, fixed Starting Strength progression a Family Member follows. There is no choice of programme template or variant — every Family Member follows the same structure, differentiated only by their own weights and current Training Phase.
_Avoid_: Programme template, plan

**Training Phase**:
One of three sequential stages of the Programme (Phase 1, 2, 3), distinguished by which lift occupies the third slot of Workout B (Deadlift, then Row, then Lat Pulldown). Workout A and the first two lifts of Workout B never change across phases. A Family Member advances to the next Training Phase manually, by their own judgment about recovery from deadlifting — the app never infers or automates this.
_Avoid_: Phase (bare, ambiguous with Programme Session sequencing), level

**Ramp**:
A one-time procedure, performed only the first time a Family Member ever does a given lift, to discover their initial Work Weight. Starts at a fixed opening weight, adds weight in configurable increments across sets of 5 reps until the lifter judges the bar has slowed, then transitions straight into that lift's work sets. Never recurs for that lift afterward.
_Avoid_: Warm-up (the Ramp is a distinct, one-time concept; the ordinary warm-up calculation that precedes every other session's work sets is a separate, unrelated calculation)

**Work Weight**:
The load a Family Member lifts for a given lift's work sets in a session. Established once by that lift's Ramp, then adjusted session-to-session by the existing progression/deload rules.
_Avoid_: Working weight, target weight

## Branding
**Colours**
#CC5500, #E2725B, #FFFFF0
