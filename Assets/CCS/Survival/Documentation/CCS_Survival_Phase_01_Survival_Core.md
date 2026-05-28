# CCS Survival — Phase 1 Survival Core

**Document Type:** Phase Engineering Plan  
**Project:** CCS Survival  
**Phase:** 1 — Survival Core  
**Author:** James Schilz  
**Date:** 2026-05-27  
**Status:** Phase 1H.5 — Overlapping Vitals Modifier Zone Testbed (Implemented)

---

## Phase 1F — Movement Polish + Stamina Hook Plan

### 1. Purpose

Connect movement feel to survival stamina without tightly coupling the movement controller to survival internals.

Phase 1E proved basic locomotion, input bindings, prototype test ground, visual readability, and standalone **0.4.0-B** validation. Phase 1F adds the first survival-aware sprint loop: stamina drain while sprinting, recovery while not sprinting, and sprint gating when depleted — while keeping movement on the player root and vitals on the bootstrap composition root.

### 2. Goals

- **Sprint** should eventually **consume stamina** while active.
- **Stamina** should **recover** when the player is not sprinting (baseline recovery rules owned by survival service/module).
- Player should **not sprint** when stamina is depleted (or should fall back to walk speed only).
- Movement controller should communicate through **`CCS_ISurvivalVitalsService`** or a **clean adapter** — not directly edit survival state fields or bypass the service boundary.
- Keep the **debug overlay** readable during sprint/recovery testing.
- Keep the controller **prototype-focused** (no final animation, camera look, or HUD polish in this pass).

### 3. Movement polish targets

- Validate **`WorldRelative`** vs **`CameraRelative`** movement using `CCS_SurvivalPrototypeMovementSpace`.
- **Keep WorldRelative for now** if it gives cleaner straight-line testing (current scene default after Phase 1E.5).
- **Return to CameraRelative** once camera look/control is implemented (Phase 1F does not implement look).
- **Smooth rotation** only if it does not reintroduce movement arc/drift during A/D validation.
- **Avoid over-polishing** locomotion feel until final third-person controller direction is locked.

### 4. Architecture

```text
PF_CCS_Survival_BootstrapRoot              CCS_PlayerRoot
├── CCS_RuntimeHost                        ├── CCS_SurvivalPrototypeCharacterController
├── CCS_SurvivalBootstrap                  │     ├── asks: CanSprint? / TryConsumeStaminaForSprint
├── CCS_SurvivalModule (vitals + stamina)  │     └── applies walk/sprint speed from input + service
└── CCS_SurvivalDebugOverlay                 └── CharacterController + camera target
```

- **Movement** remains on **`CCS_PlayerRoot`**.
- **Survival vitals** (including stamina) remain on **`PF_CCS_Survival_BootstrapRoot`** via `CCS_SurvivalModule` / `CCS_ISurvivalVitalsService`.
- **Stamina changes** belong to the survival module/service — not inside the movement MonoBehaviour as hidden state.
- Movement **asks** whether sprint is allowed and **requests** stamina use (read/query + thin write API on service); survival module applies drain/recovery rules.
- **Do not** create direct hidden object dependencies, `FindObjectOfType`, or scene searches in `Update`.
- Prefer explicit references (serialized service host, injected adapter, or registry resolve once at enable) over global singletons.

### 5. Deferred

- Final animation
- Final camera look / aim
- Final stamina UI
- Dynamic glyph UI
- Multiplayer ownership / replication
- Inventory or load-weight movement penalties

### 6. Done criteria

- [x] Sprint **drains stamina** while sprint input is held and the player is moving (Phase 1F.1).
- [x] Stamina **recovers** when not sprinting (per profile tuning; owned by `CCS_SurvivalModule`).
- [x] Sprint **stops or falls back to walk** when stamina is empty or below minimum threshold.
- [x] Movement still works with **keyboard/gamepad** (existing Input Actions unchanged in spirit).
- [x] `CCS_ISurvivalVitalsService` still registers on bootstrap root (**`Services=1`**).
- [x] Survival debug overlay remains readable; no per-tick health log spam.
- [x] No direct survival state mutation from movement code outside service contracts.

### 7. Standalone build checkpoint

After Phase 1F.1, create a follow-up standalone smoke build (**0.4.0-C** recommended) to validate stamina gating and overlay outside the Editor.

---

## Implementation Status (Phase 1F.1)

- Sprint now **consumes stamina** through **`CCS_ISurvivalVitalsService`** (`HasStamina`, `TryConsumeStamina`, `RestoreStamina`, `CurrentStamina`).
- **`CCS_SurvivalPrototypeCharacterController`** resolves the vitals service once at startup (optional serialized `CCS_RuntimeHost`, otherwise one scene lookup); no per-frame scene search.
- Movement does **not** directly own or mutate **`CCS_SurvivalState`**; stamina recovery remains owned by **`CCS_SurvivalModule`** (`RecoverStamina` in `Update`).
- Scene tuning on **`SCN_CCS_Survival_Bootstrap`**: `sprintStaminaCostPerSecond` **18**, `minimumStaminaToSprint` **5**, `movementSpace` **WorldRelative**.
- Debug overlay **`STM`** line continues to reflect stamina while sprinting and recovering (not final UI).
- Standalone build **0.4.0-C** recommended after Play Mode validation.

### Phase 1F.1 manual validation

1. Open `SCN_CCS_Survival_Bootstrap.unity` → Play Mode
2. **WASD** moves; **Left Shift** sprint drains stamina on the overlay
3. Sprint falls back to walk when stamina is depleted or below minimum
4. Stamina recovers when sprint is released
5. Console **`Services=1`**; no errors; missing-service fallback allows sprint with a one-time warning only when vitals are absent

---

## Implementation Status (Phase 1F.2)

- **`CCS_PrototypeEnvironmentRoot`** added to organize static prototype geometry under one parent.
- **`CCS_PrototypeGridRoot`** grouped grid-line cubes (removed in Phase 1F.3; see ground grid material).
- Traversal test course added with simple primitives:
  - **`CCS_PrototypeStep`** — step-offset validation (`stepOffset` 0.3)
  - **`CCS_PrototypeRamp`** — slope validation (~22°, within `slopeLimit` 45)
  - **`CCS_PrototypeStairsRoot`** — six 0.25m stair blocks for repeated elevation changes
  - **`CCS_PrototypeBoundaryMarkers`** — subtle corner reference posts
- Materials: `MAT_CCS_Prototype_Ramp_Blue`, `MAT_CCS_Prototype_Stairs_Orange` (existing ground/step/grid/player materials unchanged).
- Scene hierarchy target:

```text
SCN_CCS_Survival_Bootstrap
├── PF_CCS_Survival_BootstrapRoot
├── Directional Light
├── CCS_PlayerRoot
├── Main Camera
├── CM_PrototypeFollow
└── CCS_PrototypeEnvironmentRoot
    ├── CCS_PrototypeGround
    ├── CCS_PrototypeGridRoot
    ├── CCS_PrototypeStep
    ├── CCS_PrototypeRamp
    ├── CCS_PrototypeStairsRoot
    └── CCS_PrototypeBoundaryMarkers
```

- Final environment art, terrain, and Gaia remain **deferred**.

### Phase 1F.2 manual validation

1. Reload `SCN_CCS_Survival_Bootstrap.unity` if Unity was open during scene update
2. **WASD** + sprint stamina still work; overlay readable
3. Walk to **step** (east of spawn), **ramp** (west), **stairs** (east/south area)
4. Camera follows; console **`Services=1`**; no errors

---

## Implementation Status (Phase 1F.3)

- Replaced **`CCS_PrototypeGridRoot`** / **`CCS_PrototypeGridLine_*`** cube objects with a tiled ground grid material on **`CCS_PrototypeGround`**.
- **`MAT_CCS_Prototype_Ground_Grid`** uses in-project **`TEX_CCS_Prototype_Ground_Grid.png`** (URP Lit, 10×10 tiling on the 40×40m plane).
- Prototype hierarchy simplified under **`CCS_PrototypeEnvironmentRoot`** (ground, step, ramp, stairs, boundary markers only).
- Removed unused materials: **`MAT_CCS_Prototype_GridLine_Dark`**, **`MAT_CCS_Prototype_Ground_Neutral`**.
- Kept: **`MAT_CCS_Prototype_Player_Yellow`**, **`MAT_CCS_Prototype_Step_Grey`**, **`MAT_CCS_Prototype_Ramp_Blue`**, **`MAT_CCS_Prototype_Stairs_Orange`**, **`MAT_CCS_Prototype_Ground_Grid`**.
- Final environment art, terrain, and Gaia remain **deferred**.

### Phase 1F.3 manual validation

1. Reload scene if Unity was open during update
2. Ground shows subtle grid in Scene/Game view; **`MeshCollider`** still on ground
3. No missing materials; traversal objects still visible
4. Movement, sprint stamina, overlay, **`Services=1`**, no console errors

---

## Implementation Status (Phase 1F.4)

- Connected traversal course: **ground → stairs → platform → ramp → ground** along **+Z** from spawn.
- **`CCS_PrototypeStairsRoot`**: 6 steps, **0.25m** rise, **0.6m** depth, **2.5m** width (total rise **1.5m**).
- **`CCS_PrototypePlatform`**: **3.5×2.8m** deck at **1.5m** height (`MAT_CCS_Prototype_Platform_Green`).
- **`CCS_PrototypeRamp`**: **22°** slope, **4.2m** run, connects platform front to ground.
- **`CCS_PrototypeStep`** remains separate for isolated step-offset testing at `(3, 0.25, 3)`.
- Final level design / environment art / terrain remain **deferred**.

### Phase 1F.4 manual validation

1. From spawn, walk to stairs at **+Z**, climb to green platform, descend blue ramp back to ground
2. No floating gaps between stair top, platform, and ramp
3. Movement, sprint/stamina, camera follow on elevation, overlay, **`Services=1`**

### Standalone build 0.4.0-C

Smoke build after Phase 1F.1–1F.4: `Builds/Windows/CCS-Survival-0.4.0-C/CCS_Survival.exe` — validates stamina sprint, grid ground material, and connected traversal course outside the Editor.

---

## Phase 1F.5 — Automated Traversal Test Agent Plan

### 1. Purpose

Add a **development-only** automated test agent that can move through the prototype traversal course to repeatedly validate:

- Movement and gravity
- Stairs, ramp, and platform transitions
- Stamina behavior during automated sprint segments (when enabled)
- **Later:** fall damage when that milestone is implemented

This is a **test harness**, not player replacement, not final AI, and not shipping gameplay.

### 2. Test route

**Primary loop (bidirectional course validation):**

1. Start near spawn
2. Move to stairs base
3. Climb stairs
4. Cross platform
5. Descend ramp to ground
6. **Return path:**
   - Climb ramp
   - Cross platform
   - Descend stairs
7. Repeat

**Future fall-damage branch (deferred):**

1. Move to platform edge (no ramp/stairs)
2. Walk off platform edge
3. Allow gravity to take over
4. Later: validate fall damage rules when implemented

### 3. Architecture

- **Prototype / test-only** component — clearly separated from gameplay systems.
- **Must not** replace normal player input as the default play experience.
- **Must not** be framed or implemented as final AI.
- Prefer reusing existing **`CharacterController`** movement logic where clean; otherwise use a **lightweight test-driver adapter** that feeds the same motor constraints (walk/sprint speeds, gravity, step offset, slope limit).
- **Avoid** scene-wide searches in `Update` (`FindObjectOfType`, reflection discovery, etc.).
- Use **serialized waypoint references** and/or a **route asset** (ScriptableObject) for explicit route data.
- Keep under a clear test namespace/path.

**Suggested future script path:**

`Assets/CCS/Survival/Runtime/Testing/Traversal/CCS_PrototypeTraversalTestAgent.cs`

**Suggested supporting types (future):**

- `CCS_PrototypeTraversalRoute` — ordered waypoint list, loop flags, optional return-path segment definitions
- `CCS_PrototypeTraversalWaypoint` — transform reference + arrival radius + optional sprint flag

```text
CCS_PlayerRoot (manual control default)
├── CCS_SurvivalPrototypeCharacterController
└── CCS_PrototypeTraversalTestAgent (disabled by default; dev/test only)
      └── reads CCS_PrototypeTraversalRoute (serialized or SO)
```

### 4. Scene organization

Suggested route parent in `SCN_CCS_Survival_Bootstrap.unity`:

```text
CCS_PrototypeTraversalRoute
├── WP_Start
├── WP_StairsBase
├── WP_PlatformTop
├── WP_RampBottom
├── WP_RampTop
├── WP_StairsTop
└── WP_FallTestEdge          (later — fall-damage branch)
```

Waypoints are **empty Transform markers** (or lightweight gizmo components) placed at course landmarks aligned with the Phase 1F.4 connected path along **+Z**.

### 5. Done criteria

- [x] Agent can **follow** the serialized route in order (Phase 1F.6).
- [x] Agent can **loop** the course when `loopRoute` is enabled (Phase 1F.6).
- [x] Route/agent can be **enabled/disabled** in the Inspector (`enableTraversalTest` default off).
- [x] **Manual player control** remains on `CCS_PlayerRoot` (separate test agent object).
- [x] No final AI behavior implied.
- [x] Logs are **concise** (start, reach, advance, loop complete) when debug enabled.
- [ ] Design supports **future test report integration** (deferred).

### 6. Deferred

- Final AI / NPC locomotion
- NavMesh pathfinding
- Combat AI
- Wildlife AI
- Fall damage implementation and validation
- Behavior trees / GOAP
- Automated test result file export (JSON/XML) — plan for later integration only

---

## Implementation Status (Phase 1F.6)

- **`CCS_TraversalTestWaypoint`**, **`CCS_TraversalTestRoute`**, **`CCS_TraversalTestAgent`** under `Assets/CCS/Survival/Runtime/Testing/Traversal/`.
- Scene objects in **`SCN_CCS_Survival_Bootstrap`**: `CCS_PrototypeTraversalRoute` (7 waypoints) + `CCS_TraversalTestAgent` (blue capsule visual, disabled by default).
- Agent uses **CharacterController** + simple gravity; follows serialized route; optional loop; no Input System.
- Manual **`CCS_PlayerRoot`** is unchanged when the test agent is disabled; traversal mode hides the player root and restores it when the test ends.

### Phase 1F.6 manual validation

1. Play Mode with **`enableTraversalTest`** off — WASD player works as before
2. Enable **`enableTraversalTest`** on `CCS_TraversalTestAgent` — agent follows spawn → stairs → platform → ramp → return
3. Confirm loop restarts when `loopRoute` is enabled
4. Console: concise `[CCS Traversal Test]` logs only when `enableDebugLogs` is on
5. With **`disableManualPlayerDuringTest`** on (default), **`CCS_PlayerRoot`** is deactivated during the test; **`CM_PrototypeFollow`** tracks **`CCS_TraversalAgentCameraTarget`** instead of the player target
6. Disable the test — **`CCS_PlayerRoot`** and player camera tracking restore; WASD works again

---

## Phase 1F.7 — Standalone Traversal Validation Build

### Purpose

Validate the Phase 1F.6 automated traversal test agent in a **Windows standalone Development** build outside the Unity Editor.

### Build

| Item | Value |
|------|--------|
| **Output** | `Builds/Windows/CCS-Survival-0.4.0-D/CCS_Survival.exe` |
| **Log** | `Logs/Build_0_4_0_D.log` (prefix `[CCS 0.4.0-D]`) |
| **Bootstrap scene** | `SCN_CCS_Survival_Bootstrap.unity` (build index **0**) |
| **Validation mode** | Traversal test enabled **only in the built player** via temporary Editor build step; committed scene keeps **`enableTraversalTest` off** by default |

### Runtime validation (automated pass)

| Item | Result |
|------|--------|
| **Duration** | ~90 seconds (headless process stop; extended 5-minute pass optional) |
| **Exception** | 0 |
| **LogError** | 0 |
| **NullReferenceException** | 0 |
| **MissingReferenceException** | 0 |
| **CharacterController warnings** | 0 |
| **Services=** | 1 |
| **Health changed** | 20 |
| **Bootstrap diagnostics** | Present (normal startup) |
| **`[CCS Traversal Test]` logs** | Present (debug enabled on agent); route loop restarts observed |

### Manual visual checklist

| Check | Status |
|-------|--------|
| Camera visible and stable | **Pending** human confirmation |
| Manual player movement (test off) | **Pending** |
| Sprint and stamina | **Pending** |
| Debug overlay readable | **Pending** |
| Traversal agent follows route in standalone | **Log-confirmed**; visual **Pending** |
| Stairs / platform / ramp stable | **Pending** |
| Loop behavior | **Log-confirmed** (`Route loop complete` ×16 in sample pass) |

### Build console notes (expected, not failures)

- **URP assets included** — informational listing of `PC_RPAsset` / `PC_Renderer`
- **Stripping Runtime Debug Shader Variants** — normal for player builds
- **`CCS.Core.Editor.asmdef` will not be compiled** — empty Editor asmdef; harmless
- **`[CCS 0.4.0-D] Build succeeded`** — success marker

Temporary **`CCS_StandaloneBuild_0_4_0_D_Editor`** auto-run scripts were **removed** after the build so the Editor does not keep re-triggering builds on script reload.

### Traversal / manual player isolation (Play Mode + standalone)

- **Traversal mode:** when **`enableTraversalTest`** is on and **`disableManualPlayerDuringTest`** is on (default), **`CCS_PlayerRoot`** is set inactive so its **CharacterController** cannot block the route.
- **Camera:** **`CM_PrototypeFollow`** switches follow/look targets between **`CCS_PlayerCameraTarget`** and **`CCS_TraversalAgentCameraTarget`** (no transform reparenting).
- **Normal player mode:** when the traversal test is off, cached active state is restored on **`CCS_PlayerRoot`** and Cinemachine targets return to the player camera target.
- **`OnDisable`** / **`OnDestroy`** restore defensively if Play Mode stops while the test is active.

### Follow-up (post 1F.7)

- Rebuild **0.4.0-D** after pulling player-isolation fixes if validating traversal in standalone again

### Result status

**Passed** automated log criteria (no errors; traversal loop logs). **Manual visual checklist** remains for Play Mode / standalone spot-check.

---

## Phase 1G — Traversal Telemetry & Runtime Validation

### Purpose

Upgrade **`CCS_TraversalTestAgent`** from a visual route follower into a lightweight **dev/test validation harness** with runtime telemetry, stuck detection, route duration limits, and concise **PASSED** / **FAILED** logging — without replacing gameplay AI or manual player testing.

### Telemetry (runtime)

Tracked on the agent during an active traversal session:

| Field | Role |
|-------|------|
| `testStartTime` | Session start (`Time.time`) |
| `currentRouteElapsedTime` | Elapsed time for the current route pass |
| `completedRouteCount` | Successful full-route completions |
| `failedRouteCount` | Failed route passes (stuck or timeout) |
| `currentWaypointIndex` | Active waypoint index |
| `lastWaypointAdvanceTime` | Time of last waypoint advance |
| `distanceToCurrentWaypoint` | Distance to active waypoint |
| `totalWaypointAdvances` | Waypoint advances this pass |
| `lastKnownPosition` | Last position used for stuck detection |
| `stuckTimer` | Time without sufficient movement |
| `routeResultStatus` | Internal idle / running / passed / failed / stopped state |

### Validation settings (Inspector)

| Setting | Default | Behavior |
|---------|---------|----------|
| `enableTelemetryLogging` | on | Start, **PASSED**, and **FAILED** messages |
| `enableStuckDetection` | on | Fail if movement &lt; `stuckDistanceThreshold` for `stuckTimeLimit` s (not while waiting) |
| `stuckDistanceThreshold` | 0.15 m | Movement required to reset stuck timer |
| `stuckTimeLimit` | 5 s | Stuck duration before failure |
| `maxRouteDurationSeconds` | 120 s | Max time per route pass |
| `logRouteSummaryOnComplete` | on | Log **PASSED** summary on completion |
| `stopTestOnFailure` | on | Stop test and restore manual player on failure |
| `enableDebugLogs` | off | Verbose per-waypoint logs (optional) |

### Pass / fail logging

| Event | Log |
|-------|-----|
| Session start | `[CCS Traversal Test] Traversal validation started.` |
| Route pass success | `[CCS Traversal Test] PASSED: Route completed in X.XXs. Waypoints=N. Loops=N.` |
| Stuck | `[CCS Traversal Test] FAILED: Agent stuck near waypoint '<name>' (index N).` |
| Timeout | `[CCS Traversal Test] FAILED: Route exceeded max duration.` |

No per-frame logging. **`enableTraversalTest`** remains **off** by default.

### Read-only debug access

Public properties on **`CCS_TraversalTestAgent`**: `CompletedRouteCount`, `FailedRouteCount`, `CurrentRouteElapsedTime`, `IsTraversalRunning`, `CurrentWaypointIndex`.

### Player isolation and camera (1F.7+)

- **Traversal off:** normal **`CCS_PlayerRoot`** + WASD; **`CM_PrototypeFollow`** uses **`CCS_PlayerCameraTarget`**
- **Traversal on:** **`CCS_PlayerRoot`** hidden; Cinemachine tracks **`CCS_TraversalAgentCameraTarget`** on the agent
- **Lifecycle-safe teardown:** no **`SetParent`** on camera targets; Play Mode exit skips Cinemachine mutations when the application is quitting or the agent is shutting down
- **Restore:** player active state and player camera target are restored on normal test stop; duplicate restore is prevented per session

### Validation checklist

1. Play Mode — **`enableTraversalTest`** off → WASD player works
2. Enable traversal test → player hidden, camera follows blue agent
3. Agent completes route → one **PASSED** summary per loop (no spam)
4. Loop route → timer resets per pass; `Loops` increments in summary
5. Disable test → player and camera restore; WASD works
6. Optional: lower `stuckTimeLimit` in Inspector to confirm **FAILED** stuck path
7. No console errors

### Implementation status

- **`CCS_TraversalTestAgent`** — telemetry, stuck detection, duration validation, pass/fail summaries
- Scene **`SCN_CCS_Survival_Bootstrap`** — validation defaults wired; `enableTraversalTest` off; `enableDebugLogs` off (telemetry on)

---

## Phase 1G.1 — Standalone Telemetry Validation Build

### Purpose

Validate Phase **1G** traversal telemetry and **`b02492b`** Cinemachine camera target switching in a **Windows standalone Development** build outside the Editor.

### Build

| Item | Value |
|------|--------|
| **Output** | `Builds/Windows/CCS-Survival-0.4.0-E/CCS_Survival.exe` |
| **Summary log** | `Logs/Build_0_4_0_E.log` (prefix `[CCS 0.4.0-E]`) |
| **Unity log** | `Logs/Build_0_4_0_E_Unity.log` (not committed) |
| **Bootstrap scene** | `SCN_CCS_Survival_Bootstrap.unity` (build index **0**) |
| **Validation mode** | Temporary Editor build step enabled **`enableTraversalTest`** for the packaged player only; committed scene restored to **`enableTraversalTest` off** after build |

### Runtime validation (automated)

| Item | Value |
|------|--------|
| **Duration** | ~60 seconds |
| **Exception** | 0 |
| **LogError** | 0 |
| **NullReferenceException** | 0 |
| **MissingReferenceException** | 0 |
| **Cannot set the parent** | 0 |
| **Traversal FAILED** (`[CCS Traversal Test] FAILED`) | 0 |
| **PASSED: Route completed** | 10 (≥ 2 required) |
| **Core health OK** | 1 |
| **Survival validation rules passed** | 1 |

Last sample pass: `PASSED: Route completed in 5.24s. Waypoints=7. Loops=10.`

### Manual visual checklist

| Check | Status |
|-------|--------|
| Camera follows traversal agent in standalone | **Pending** human confirmation |
| Player hidden during traversal test | **Log/behavior expected**; visual **Pending** |
| Blue agent completes route | **Log-confirmed** (10 PASSED summaries in 60s run) |
| No blocking at route start | **Pending** |
| No obvious camera freeze | **Pending** |
| Debug overlay readable | **Pending** |

### Camera / teardown

- Standalone run produced **no** `Cannot set the parent` errors (camera target switching validated in logs).
- Play Mode exit teardown with camera switching remains validated separately in Editor.

### Result status

**Passed** automated Player.log criteria for standalone telemetry validation. Manual visual checklist **pending**.

---

## Phase 1H — Survival Gameplay Direction Lock

### 1. Purpose

Before adding more gameplay systems, lock a **foundational survival direction** for **CCS Survival** so future modules, scenes, and milestones align around one prototype target and one readable gameplay loop.

Phase **1A–1G.1** proved Core bootstrap, vitals, movement, stamina, traversal course validation, and standalone telemetry. Phase **1H** is **planning only** — no major gameplay implementation in this milestone.

This section is a **living gameplay direction document** and may expand as playtests inform priorities.

---

### 2. Core survival pillars

Intended pillars for the Phase 1 prototype and near-term survival core:

| Pillar | Direction (prototype) |
|--------|------------------------|
| **Traversal & environmental movement** | Ground-based travel with readable elevation (steps, ramps, stairs); stamina-aware sprint; future hazards tied to terrain zones |
| **Exploration** | Short-loop discovery of landmarks, resources, and risk — not open-world scale yet |
| **Survival pressure** | Vitals (health, stamina, hunger/thirst placeholders) create **decisions**, not instant fail states |
| **Stamina / resource management** | Sprint and exertion consume recoverable stamina; future consumables extend the same service boundaries |
| **Inventory pressure** | Limited carry capacity and item scarcity drive route planning (later milestone) |
| **Environmental danger** | Heat, cold, fall, exposure, and hazard volumes as **readable** threats before complex simulation |
| **Shelter / safe zones** | Recovery spaces that reduce pressure or pause certain drains (camp, interior, fire — phased) |
| **Progression pacing** | Early sessions = learn loop; mid prototype = optimize routes; later = unlock tools/structures |
| **Realism vs accessibility** | **Grounded frontier survival** with **readable feedback** (overlay, audio, UI) — avoid opaque simulation |
| **Immersive simulation vs arcade balance** | Prefer **tactical readability** over hardcore micromanagement in Phase 1; deepen simulation only where it improves decisions |

**Design bias:** pressure should be **felt** through vitals, environment, and inventory — not through unfair hidden rules.

---

### 3. Intended player fantasy

The prototype should communicate this experience:

| Theme | Notes |
|-------|--------|
| **Lone survivor** | Single-player focus first; systems avoid hidden global singletons to stay multiplayer-conscious |
| **Western / frontier realism** | Tone and content live in **ccs-survival** (Reckoning product lore); not framework branding |
| **Harsh environmental survival** | Weather, exposure, and terrain matter; death/recovery rules stay understandable |
| **Tactical resource management** | Stamina today; food/water/tools tomorrow — plan routes and recovery |
| **Exploration-driven progression** | Learn the space, find better resources, establish safer loops |
| **Camp / shelter building** | Deferred as a structured milestone; directionally part of the fantasy |
| **Emergent encounters** | Wildlife, NPCs, and events **later** — architecture leaves hooks, no premature AI scope |
| **Multiplayer future compatibility** | Service-owned state, explicit ownership, save-stable IDs — no netcode in Phase 1 |

**Player sentence (north star):** *Survive the frontier by reading the land, managing your body, and making deliberate trips between risk and recovery.*

---

### 4. Prototype success criteria

Phase 1 should **prove** the following before expanding scope:

| Criterion | Meaning |
|-----------|---------|
| **Traversal feels good** | Connected course + real player control; automated traversal harness catches regressions |
| **Survival pressure creates decisions** | Sprint gating, vitals drift, and future hazards change player choices |
| **Player understands vitals** | Debug overlay (then HUD) makes cause/effect readable |
| **Environment influences gameplay** | Zones, shelter, and terrain modifiers affect survival — not cosmetic-only world |
| **Gameplay loop stays readable** | One core loop document; systems do not fight each other |
| **Systems are modular and scalable** | Modules/services per CCS platform rules; gameplay stays in **ccs-survival** |

**Not required in Phase 1:** MMO scale, full crafting trees, combat depth, or final art pass.

---

### 5. Candidate early survival systems

Categorized backlog for planning. Items may move as milestones are reprioritized.

#### Core survival

| System | Purpose | Priority |
|--------|---------|----------|
| Vitals (health, stamina, hunger/thirst) | Central survival pressure and recovery | **Recommended Early** (health/stamina in progress) |
| Vitals tuning & failure rules | Readable drain/recovery; optional downed state later | **Recommended Early** |
| Consumables (food/water/medical) | Restore vitals; first crafting-adjacent loop | **Later / Deferred** |
| Status effects (bleeding, cold, heat) | Layered pressure from environment/combat | **Later / Deferred** |

#### Environment

| System | Purpose | Priority |
|--------|---------|----------|
| Hazard volumes (cold, heat, damage) | Environmental danger with clear boundaries | **Done** (Phase 1H.1) |
| Day / night cycle (prototype) | Pacing and exposure pressure | **Later / Deferred** |
| Weather (lightweight) | Atmosphere + survival modifiers | **Later / Deferred** |
| Fall damage | Vertical hazard validation on platform course | **Later / Deferred** (branch planned near traversal route) |

#### Traversal

| System | Purpose | Priority |
|--------|---------|----------|
| CharacterController locomotion | Player movement baseline | **Done** (Phase 1E+) |
| Stamina-linked sprint | Survival-aware movement | **Done** (Phase 1F+) |
| Traversal test harness | Automated route validation | **Done** (Phase 1F.6–1G.1) |
| Climb / mantle / vehicles | Advanced locomotion | **Later / Deferred** |

#### Inventory

| System | Purpose | Priority |
|--------|---------|----------|
| Item definitions (data) | Save-stable IDs, module-friendly | **Recommended Early** |
| Pickup & carry | Gather supplies in the world | **Recommended Early** |
| Inventory UI (prototype) | Readability for testing | **Later / Deferred** |
| Equipment slots | Wearables affecting survival stats | **Later / Deferred** |

#### Interaction

| System | Purpose | Priority |
|--------|---------|----------|
| Interactable framework | Use/open/harvest/pickup contracts | **Recommended Early** |
| World containers & stations | Stash, campfire, workbench placeholders | **Later / Deferred** |
| Dialogue / quests | Narrative systems | **Later / Deferred** |

#### AI / wildlife

| System | Purpose | Priority |
|--------|---------|----------|
| Passive wildlife placeholders | Atmosphere only | **Later / Deferred** |
| Threat wildlife / NPC combat | Combat loop | **Later / Deferred** |
| Traversal test agent | Dev validation only — **not** gameplay AI | **Done** (dev-only) |

#### World simulation

| System | Purpose | Priority |
|--------|---------|----------|
| Spawn & landmark markers | Readable prototype spaces | **In progress** (prototype course) |
| Resource nodes (respawn rules) | Gather loop | **Later / Deferred** |
| Faction / territory simulation | Product-scale MMO direction | **Later / Deferred** |

#### Save / progression

| System | Purpose | Priority |
|--------|---------|----------|
| Session vitals persistence | Continue survival state | **Later / Deferred** |
| Player progression / unlocks | Long-term goals | **Later / Deferred** |
| World state persistence | Structures, containers, map | **Later / Deferred** |

#### Multiplayer future considerations

| System | Purpose | Priority |
|--------|---------|----------|
| Authority-safe service ownership | Host/client-ready boundaries | **Recommended Early** (ongoing discipline) |
| Deterministic simulation hooks | Future replication | **Later / Deferred** |
| Netcode / replication | Actual multiplayer | **Later / Deferred** |

---

### 6. Recommended initial gameplay loop

First-pass **high-level** loop for the survival prototype (iterative, not final design):

```text
Spawn / wake at safe point
    → Orient (vitals overlay / landmarks)
    → Explore (stamina-aware traversal)
    → Gather or interact (pickup / resources — when implemented)
    → Face environmental pressure (hazard zones, exposure, future fall damage)
    → Manage vitals (rest, consume, retreat)
    → Recover at shelter / safe zone (reduced drain or faster recovery)
    → Plan next sortie (inventory + stamina + route)
    → Repeat
```

**Phase 1 slice:** prove **explore → pressure → recover** with movement, vitals, and one environmental hazard before adding deep crafting or combat.

---

### 7. Technical direction notes

Align gameplay implementation with CCS platform rules already in use:

| Topic | Direction |
|-------|-----------|
| **Event-driven communication** | `CCS_EventDispatcher` and service callbacks for cross-system signals |
| **Modular services / modules** | `CCS_IModule`, `CCS_ISurvivalVitalsService`, manual install plans — no scene-wide discovery |
| **Save-stable IDs** | Item, hazard, and zone identifiers suitable for future persistence |
| **Multiplayer-conscious patterns** | Instance-owned hosts; avoid static global gameplay state |
| **Low coupling** | Movement asks vitals service; hazards publish events; UI reads services |
| **CharacterController traversal** | Prototype locomotion stays controller-based (not Rigidbody gameplay motor) |
| **Telemetry / test-first** | Traversal harness pattern extends to future automated validation (hazards, vitals thresholds) |
| **Dev vs shipping** | Test agents, debug overlays, and validation builds stay clearly dev-scoped |

**Repository boundary:** reusable platform in **ccs-framework**; survival gameplay in **ccs-survival** only.

---

### 8. Immediate recommended next milestones

Proposed roadmap **after Phase 1H** (order may change after playtests):

| Milestone | Focus |
|-----------|--------|
| **1H.1** | Environmental hazard zones (volume-based cold/heat/damage prototype) — **implemented** |
| **1H.2** | Survival vitals balancing pass (readable drain/recovery tuning) |
| **1H.3** | Basic item pickup (data + interact hook, no full UI) |
| **1H.4** | Inventory module integration (carry limits, service API) |
| **1H.5** | Shelter / safe zone recovery (modifier volumes or stations) |
| **1H.6** | Basic interaction framework (shared interactable contract) |
| **1H.7** | Day/night prototype (lighting + survival modifier, lightweight) |
| **1H.8** | Standalone validation build per milestone (same pattern as **0.4.0-E**) |

**Gate before large features:** each milestone should keep **manual player mode** intact and extend automated validation where practical.

---

### 9. Phase 1H done criteria (planning)

- [x] Core survival pillars documented
- [x] Player fantasy and prototype success criteria defined
- [x] Candidate systems categorized (early vs deferred)
- [x] Initial gameplay loop drafted
- [x] Technical direction aligned with CCS architecture
- [x] Next-milestone roadmap proposed
- [ ] Implementation of new gameplay systems (explicitly **out of scope** for 1H)

### Result status

**Planning complete.** Implementation milestones begin at **1H.1** or reprioritized equivalent after review.

---

## Phase 1H.1 — Environmental Hazard Zones

### Purpose

Introduce the first **gameplay-pressure** milestone: modular **environmental hazard zones** that influence survival vitals through existing service boundaries — without weather simulation, combat, or complex VFX.

### Architecture

```text
CCS_SurvivalHazardZone / CCS_SurvivalSafeZone (trigger volumes)
        ↓ enter/exit
CCS_SurvivalHazardReceiver (per entity: player, traversal agent, future avatars)
        ↓ applies pressure while inside
CCS_ISurvivalVitalsService (CCS_SurvivalModule on bootstrap root)
```

- **Low coupling:** zones do not reference the player directly; they notify receivers on trigger overlap.
- **Multiplayer-conscious:** one receiver per authority-owned body; vitals service remains on session/bootstrap host.
- **Traversal preserved:** test agent has a receiver with **telemetry-only** logging (`applyToSurvivalVitals` off).

### Scripts (`Assets/CCS/Survival/Runtime/Environment/Hazards/`)

| Script | Role |
|--------|------|
| `CCS_SurvivalHazardType` | Cold, Heat, Toxic, Radiation (placeholder), GenericDamage |
| `CCS_SurvivalHazardProfile` | Optional ScriptableObject preset for zone tuning |
| `CCS_SurvivalHazardZone` | Trigger hazard volume with rates + gizmo color |
| `CCS_SurvivalSafeZone` | Suppresses hazard pressure; optional recovery |
| `CCS_SurvivalHazardReceiver` | Aggregates active zones; applies vitals pressure |

### Supported hazard pressure

| Channel | Use |
|---------|-----|
| Health damage / sec | Toxic, generic damage |
| Exposure / sec | Cold (feeds existing exposure damage on module) |
| Stamina drain / sec | Cold exertion, heat stress |
| Temperature change / sec | Cold / heat directional pressure |

### Safe zone behavior

While inside an active safe zone, **hazard pressure is suppressed**. Optional recovery:

- Health recovery per second
- Stamina recovery per second
- Exposure reduction or **clear exposure** while inside

### Vitals integration

- Extended **`CCS_ISurvivalVitalsService`** with `SetBodyTemperature` and `SetExposure` for hazard receivers.
- Receiver uses existing `ApplyDamage`, `TryConsumeStamina`, `RestoreHealth`, `RestoreStamina` — no duplicate vitals state.

### Telemetry (concise)

When enabled on a receiver:

```text
[CCS Survival Hazard] Entered Cold zone.
[CCS Survival Hazard] Exited Cold zone.
```

Traversal test agent enables hazard telemetry; player receiver keeps telemetry off by default.

### Scene setup (`SCN_CCS_Survival_Bootstrap`)

| Object | Notes |
|--------|--------|
| `CCS_PrototypeHazardsRoot` | Parent for prototype volumes |
| `HZ_ColdApproach` | Intersects traversal path near stairs approach |
| `HZ_ToxicPlatform` | Toxic pressure near platform segment |
| `SZ_SpawnShelter` | Safe recovery volume west of spawn |
| `CCS_PlayerRoot` | `CCS_SurvivalHazardReceiver` (vitals on) |
| `CCS_TraversalTestAgent` | `CCS_SurvivalHazardReceiver` (telemetry only) |

Gizmo colors: cold = blue, heat = orange, toxic = green, safe = cyan.

### Validation checklist

| Check | Status |
|-------|--------|
| Player enters hazard → vitals react | **Pending** Play Mode |
| Exit hazard → normal drain resumes | **Pending** |
| Safe zone suppresses / recovers | **Pending** |
| Traversal agent logs enter/exit when test on | **Pending** |
| Traversal telemetry still passes | **Pending** standalone |
| No console errors | **Pending** |

### Future extensibility

- Per-zone hazard profiles and data-driven tables
- Authority-filtered receivers for multiplayer
- Wildlife/AI receivers sharing the same zone components
- Weather and visual FX layered on top of zone contracts (not replacing them)

### Result status

**Implemented** in repo. Play Mode / standalone validation pending human or automated pass.

---

## Phase 1H.3 — Vitals Debug Isolation + Test Mode Controls

### Purpose

Phase 1H.2 showed traversal and hazard zones work, but **`CCS_SurvivalModule.Update`** still applies global hunger/thirst drain and **`ApplyEnvironmentalDamage`** on the shared vitals service. That noise made hazard-only validation hard to read in the console.

Phase 1H.3 adds **dev/test-only** controls so traversal validation can **isolate global vitals ticking** while keeping normal player gameplay unchanged when isolation is off.

### Why isolation was needed

| Symptom | Cause |
|---------|--------|
| Health drops during traversal with agent `applyToSurvivalVitals` off | Single `CCS_ISurvivalVitalsService`; module keeps simulating starvation/dehydration/exposure damage |
| Hazard telemetry looks correct but vitals logs confuse validation | `[CCS Survival Vitals] Health changed` from module tick, not hazard receiver `ApplyDamage` |

### What was isolated (when enabled)

On **`CCS_SurvivalModule`** → **Traversal Validation (Dev)** → **`vitalsTestIsolation`**:

| Toggle | Effect while traversal validation is active |
|--------|---------------------------------------------|
| `enableTraversalValidationIsolation` | Master switch (off = normal gameplay always) |
| `pauseGlobalVitalsTickDuringTraversalTest` | Pauses hunger/thirst drain, stamina recovery, and environmental damage |
| `disableEnvironmentalDamageDuringTraversalTest` | If global tick is not fully paused, skips `ApplyEnvironmentalDamage` only |
| `suppressVitalsDebugLogsDuringTraversalTest` | Hides module vitals debug logs during traversal |
| `resetVitalsOnTestStart` | Resets vitals to profile defaults when validation starts |

### What stays active during traversal validation

- **`CCS_TraversalTestAgent`** route movement, stuck detection, PASSED/FAILED telemetry
- **`CCS_SurvivalHazardReceiver`** enter/exit logs on the agent (`enableHazardTelemetryLogging`)
- Hazard receiver **vitals application** on the player when playing manually (`applyToSurvivalVitals` on `CCS_PlayerRoot`)
- Manual damage/heal/food/water APIs on `CCS_ISurvivalVitalsService` (isolation only affects module `Update` tick paths)

### Notification path (decoupled)

```text
CCS_TraversalTestAgent
  → Try CCS_ISurvivalVitalsTestModeService.NotifyTraversalValidationActive
  → else dispatch CCS_SurvivalTraversalValidationLifecycleEvent on CCS_RuntimeHost.EventDispatcher
CCS_SurvivalModule subscribes + implements test mode service
```

No singletons. Optional serialized `CCS_RuntimeHost` on the agent; falls back to a single scene `CCS_RuntimeHost` lookup for the dev harness only.

### Scene / prefab defaults

| Asset | Default |
|-------|---------|
| `PF_CCS_Survival_BootstrapRoot` | `enableTraversalValidationIsolation` **off** (normal gameplay) |
| `SCN_CCS_Survival_Bootstrap` | Scene override: isolation **on** for validation Play Mode |
| `CCS_TraversalTestAgent` | `notifyVitalsTestMode` **on** |

Committed gameplay: **`enableTraversalTest` off**, isolation master off on prefab unless scene override is intentional for local validation.

### Validation checklist

| Check | Expected |
|-------|----------|
| Traversal on + scene isolation on | Hazard enter/exit logs; route **PASSED**; no global vitals health spam |
| Traversal off | Full hunger/thirst/environmental damage; player hazards apply normally |
| Isolation master off on module | Traversal does not pause global vitals even when test runs |
| Stop / disable traversal | Vitals tick and logs restore |
| Console | No errors on play/stop |

### Scripts

| Script | Role |
|--------|------|
| `CCS_SurvivalVitalsTestIsolationSettings` | Serializable dev toggles |
| `CCS_ISurvivalVitalsTestModeService` | Service notify API |
| `CCS_SurvivalTraversalValidationLifecycleEvent` | Event fallback |
| `CCS_SurvivalModule` | Isolation + service + event subscription |
| `CCS_TraversalTestAgent` | Start/stop notifications |

### Result status

**Implemented** in repo. Play Mode validation recommended with scene isolation override enabled.

---

## Phase 1H.4 — Survival Debug UI Vitals Expansion

### Purpose

Expand the temporary **`CCS_SurvivalDebugOverlay`** (upper-right OnGUI panel) so hazard and vitals validation can be measured **visually** during Play Mode and standalone tests — without final HUD art or a UI framework.

Complements Phase 1H.3 isolation: console vitals logs may be suppressed during traversal, but the overlay remains the single at-a-glance readout.

### Added UI fields

| Line | Source |
|------|--------|
| `Temp` | `CCS_SurvivalState.BodyTemperature` |
| `Exposure` | `CCS_SurvivalState.Exposure` |
| `Hazard` | `CCS_SurvivalHazardReceiver.GetActiveHazardSummary()` — `None` / `Cold` / `Toxic` / `Multiple` |
| `Safe` | `CCS_SurvivalHazardReceiver.IsSafeZoneActive` — `Yes` / `No` |
| `Test Iso` | `CCS_ISurvivalVitalsTestModeService.IsTraversalVitalsIsolationActive` — `On` / `Off` |

Existing lines unchanged: **HP**, **Food**, **Water**, **STM**, **Alive**.

Example:

```text
Survival
HP 100
Food 81
Water 76
STM 100
Temp 37.0C
Exposure 0.0
Hazard None
Safe No
Test Iso Off
Alive
```

### Data access (no duplicate vitals model)

- **Vitals:** `CCS_SurvivalModule` / `CCS_ISurvivalVitalsService.CurrentState`
- **Hazards:** active `CCS_SurvivalHazardReceiver` — prefers traversal agent receiver when player root is inactive; otherwise player receiver
- **Isolation:** `CCS_ISurvivalVitalsTestModeService` on the same module or service registry

Read-only additions on hazard receiver: `IsSafeZoneActive`, `AppliesToSurvivalVitals`, `GetActiveHazardSummary()`.

### Debug-only status

- **Not** production HUD
- Top-right anchor, semi-transparent panel, no console spam from overlay
- No changes to hazard pressure, traversal telemetry, or vitals isolation behavior

### Validation checklist

| Check | Expected |
|-------|----------|
| Player mode | Core vitals drain/change; hazard/safe lines update on zone enter/exit |
| Cold / toxic zones | `Hazard` shows type; `Exposure` rises when player receiver applies vitals |
| Safe zone | `Safe Yes`; hazard summary clears while inside |
| Traversal test | `Test Iso On` when scene isolation enabled; agent hazard telemetry unchanged |
| Console | No new errors from overlay |

### Result status

**Implemented** in repo (`CCS_SurvivalDebugOverlay` + hazard receiver summary API).

---

## Phase 1H.5 — Overlapping Vitals Modifier Zone Testbed

### Purpose

Prototype **overlapping environmental and vitals modifier zones** so manual play and traversal validation can test **multiple simultaneous pressures** without weather simulation, inventory, or final HUD work.

**Broad box zones** represent large environmental contexts (weather-like cold, toxic cloud, shelter restore field). **Nested capsule/cylinder zones** represent direct resource modifier fields (hunger/thirst/stamina/exposure).

Existing **`CCS_SurvivalHazardZone`** volumes are preserved for hazard pressure; modifier zones are a separate layer.

### Architecture

```text
CCS_SurvivalVitalsModifierZone (trigger volumes)
        ↓ enter/exit
CCS_SurvivalVitalsZoneReceiver (per entity: player, traversal agent)
        ↓ applies rates while inside
CCS_ISurvivalVitalsService (CCS_SurvivalModule)
```

- **No duplicate vitals state** — all mutations go through the bootstrap vitals service.
- **Traversal agent** uses `applyToSurvivalVitals` off + telemetry on (same pattern as hazard receiver).
- **Phase 1H.3 isolation** unchanged — global tick pause is independent of zone receivers.

### Modifier types (`CCS_SurvivalVitalsModifierType`)

| Category | Types |
|----------|--------|
| Hunger / thirst | `HungerDrain`, `HungerRestore`, `ThirstDrain`, `ThirstRestore` |
| Stamina | `StaminaDrain`, `StaminaRestore` |
| Exposure / temp | `ExposureIncrease`, `ExposureRecovery`, `TemperatureIncrease`, `TemperatureDecrease` |
| Health | `HealthDrain`, `HealthRestore` |

Each zone supports: rate per second, optional min/max vital clamp, enable toggle, telemetry toggle, gizmo color, display label.

### Scene layout (`SCN_CCS_Survival_Bootstrap`)

Root: **`CCS_PrototypeVitalsZonesRoot`**

| Zone | Shape | Role |
|------|-------|------|
| `VZ_WeatherCold_Box` | Box | Broad cold context (`TemperatureDecrease`) overlapping cold approach |
| `VZ_ToxicCloud_Box` | Box | Broad toxic cloud (`ExposureIncrease`) overlapping platform area |
| `VZ_Shelter_Box` | Box | Broad shelter restore field near spawn |
| `VZ_HungerDrain_Capsule` | Capsule | Fast hunger drain on approach path |
| `VZ_HungerRestore_Capsule` | Capsule | Hunger restore inside shelter overlap |
| `VZ_ThirstDrain_Cylinder` | Capsule | Thirst drain along approach |
| `VZ_ThirstRestore_Cylinder` | Capsule | Thirst restore in shelter |
| `VZ_StaminaDrain_Capsule` | Capsule | Stamina drain on platform approach |
| `VZ_StaminaRestore_Capsule` | Capsule | Stamina restore in shelter |
| `VZ_ExposureRecovery_Cylinder` | Capsule | Exposure recovery overlapping shelter |

**`SZ_SpawnShelter`** (hazard safe zone) remains for hazard suppression/recovery.

Traversal route is unchanged (7 waypoints); existing path intersects cold approach, platform, and spawn shelter overlaps.

**Scene setup:** menu **CCS → Survival → Setup Phase 1H.5 Vitals Modifier Testbed** (`CCS_VitalsModifierZoneSceneSetup_Editor`). Idempotent; re-run after pulling if the root is missing.

### Vitals service extensions

`CCS_ISurvivalVitalsService` adds explicit drain/restore helpers (`DrainHunger`, `RestoreHunger`, `DrainThirst`, `RestoreThirst`, `DrainStamina`, `AddExposure`, `ReduceExposure`, `ModifyBodyTemperature`, and setters for clamping).

### Debug UI (Phase 1H.4 + 1H.5)

`CCS_SurvivalDebugOverlay` adds **`Modifier`** line (e.g. `HungerDrain + ThirstDrain` or `None`).

### Telemetry

```text
[CCS Survival Vitals Zone] Entered HungerDrain zone.
[CCS Survival Vitals Zone] Exited HungerDrain zone.
```

Enter/exit only when `enableVitalsZoneTelemetryLogging` is on (traversal agent default).

### Validation checklist

| Check | Expected |
|-------|----------|
| Hunger drain capsule | Food decreases faster while inside |
| Restore zones in shelter | Food/Water/STM recover; exposure can drop |
| Overlap broad + nested | Combined modifiers visible on overlay |
| Hazard zones | Still function; `Hazard` line unchanged |
| Traversal | Route **PASSED**; concise zone logs; `Test Iso` behaves per 1H.3 |
| Console | No errors |

### Future connection

- Weather systems can drive broad box rates instead of static prototype values.
- Items/consumables can call the same vitals service helpers.
- Authority-filtered receivers for multiplayer.

### Result status

**Implemented** in repo. Run scene setup menu once if `CCS_PrototypeVitalsZonesRoot` is not in the scene.

---

## Phase 1E — Player Movement + New Input System Plan

### 1. Purpose

Add minimum playable movement to the survival prototype so the player can move around the Phase 1D scene while survival vitals continue running on the bootstrap composition root.

Phase 1D proved camera framing, placeholder presence, and vitals overlay in Game View and standalone **0.4.0-A**; Phase 1E adds locomotion without coupling movement to survival state ownership.

### 2. Architecture

- **Player movement** belongs on **`CCS_PlayerRoot`** or a future player prefab — **not** on `PF_CCS_Survival_BootstrapRoot`.
- **Survival vitals** stay on the bootstrap root (`CCS_SurvivalModule`, `CCS_ISurvivalVitalsService`, debug overlay).
- Movement may **query** survival or stamina through `CCS_ISurvivalVitalsService` in a later pass (e.g. sprint gating) but must **not** own or mutate survival state directly.
- Use **event/service boundaries** — no vitals logic inside input or motor MonoBehaviours beyond thin service calls.
- Keep **multiplayer-conscious ownership** (player root = locomotion authority placeholder; bootstrap = session/module host) but **do not** implement networking or replication.

```text
PF_CCS_Survival_BootstrapRoot          CCS_PlayerRoot (scene / future prefab)
├── CCS_RuntimeHost                    ├── CharacterController + movement driver
├── CCS_SurvivalBootstrap              ├── CCS_PlayerCameraTarget
├── CCS_SurvivalModule (vitals)        └── PlaceholderVisual
└── CCS_SurvivalDebugOverlay

CM_PrototypeFollow → CCS_PlayerCameraTarget (unchanged from Phase 1D)
```

### 3. Movement Direction

- Use Unity **`CharacterController`** for prototype locomotion.
- **Do not** use Rigidbody-based locomotion.
- **Root motion OFF** — all displacement from scripted/controller movement.
- **Prototype scope only:**
  - Walk / move
  - **Sprint** optional if low risk (may later consult stamina via service)
  - Gravity and simple grounding via `CharacterController.isGrounded`
  - **No jump** unless trivial and isolated (default: **deferred**)
  - **No crouch**, **no combat**, **no animation controller** yet

### 4. Input Direction

- Use the **Unity New Input System** (`com.unity.inputsystem`).
- Create a survival prototype **Input Actions** asset during **Phase 1E implementation** (not in this planning pass).
- **Proposed asset path:** `Assets/CCS/Survival/Settings/Input/CCS_Survival_InputActions.inputactions`
- **Proposed action map:** `Gameplay`
- **Proposed actions:**

| Action | Phase 1E |
|--------|----------|
| **Move** | In scope |
| **Look** | In scope (may drive camera later; movement can defer look) |
| **Sprint** | In scope (optional / low risk) |
| **Jump** | Optional / **deferred** by default |
| **Interact** | Optional / **deferred** |

Wire input through a small player-side component; do not put Input Actions or `PlayerInput` on the bootstrap root.

#### Input device glyph direction (Phase 1E.1)

- Support **mouse/keyboard** and **gamepad** through the same Input Actions asset and `Gameplay` map bindings.
- **Track last-used input device live** during gameplay (keyboard/mouse vs gamepad), not only at startup.
- **UI prompts and control glyphs** must update to match the **current** active device when UI is shown.
- Device selection must be based on the **most recent meaningful input action** (move, look, sprint, interact, etc.) — not whichever device was connected or used when the scene loaded.
- **Final glyph UI** (sprites, layout, localization) is **deferred**, but input/player architecture must expose a stable hook (e.g. active device changed event or service) so UI can subscribe without rewriting movement code later.

### 5. Cinemachine Direction

- Keep **Cinemachine 3.1.6** and existing **`CM_PrototypeFollow`** as the prototype camera.
- **Target remains** `CCS_PlayerCameraTarget` on the player hierarchy.
- Movement should become **camera-relative** when look/yaw is available (standard third-person feel).
- **First implementation** may use simple **world-relative** movement on WASD only if explicitly documented as **temporary**; follow up with camera-relative facing in the same phase or immediate sub-pass.

### 6. Done Criteria

- [x] Player can **move** in Play Mode in `SCN_CCS_Survival_Bootstrap`
- [x] **Camera follows** the player (`CM_PrototypeFollow` → `CCS_PlayerCameraTarget`)
- [x] **Survival debug overlay** remains visible and readable (top-right)
- [x] `CCS_ISurvivalVitalsService` still registers on bootstrap root (**`Services=1`**)
- [x] No final UI, inventory, combat, or networking
- [x] Scene stays prototype-focused (no animation controller, no combat systems)

### 7. Standalone Build

After Phase 1E **implementation**, create standalone build checkpoint **0.4.0-B** to validate movement, camera follow, and survival overlay **outside** the Unity Editor (same smoke pattern as **0.4.0-A**).

---

## Phase 1D — Character + Camera Integration Plan

### 1. Purpose

Connect the survival vitals prototype to an actual playable scene with a player character and camera. Phase 1A–1C proved service registration, profile-driven tuning, and debug visibility on a composition root; Phase 1D adds the minimum scene presence needed to validate survival pressure in a real Game View context.

### 2. Goals

- Add a simple player character/root object
- Add a gameplay camera using **Cinemachine 3.1.6** (`com.unity.cinemachine`)
- Keep survival module on the bootstrap/composition root (not on player or camera)
- Avoid coupling vitals directly to camera or input
- Prepare for New Input System integration soon
- Keep systems multiplayer-conscious but not networked yet

### 3. Camera Foundation — Cinemachine

**Cinemachine is the chosen camera foundation** for the survival prototype and for future third-person controller work. Phase 1D uses a single follow camera only; advanced camera modes are deferred.

| Package | Version |
|---------|---------|
| `com.unity.cinemachine` | **3.1.6** |

**Cinemachine 3 naming (implementation reference)**

| Component | Role |
|-----------|------|
| `CinemachineBrain` | On **Main Camera**; blends/routes Cinemachine Camera output to Game View |
| `CinemachineCamera` | One prototype rig in the scene; follow/look-at targets drive framing |

Do not use legacy Cinemachine 2 `CinemachineVirtualCamera` setup patterns in new survival scenes.

### 4. Preferred Architecture

```text
SCN_CCS_Survival_Bootstrap.unity

PF_CCS_Survival_BootstrapRoot (composition root — unchanged)
├── CCS_RuntimeHost
├── CCS_SurvivalBootstrap
├── CCS_SurvivalModule          ← vitals service; stays here, not on player/camera
└── CCS_SurvivalDebugOverlay

Main Camera
└── CinemachineBrain            ← required on Main Camera

CM_PrototypeFollow (CinemachineCamera)
├── Follow  → CCS_PlayerCameraTarget
└── Look At → CCS_PlayerCameraTarget (or same target for minimal prototype)

CCS_PlayerRoot (placeholder)
├── Placeholder visual (capsule/primitive or minimal prefab mesh)
├── CCS_PlayerCameraTarget      ← child transform; camera aim/follow anchor
└── (future) character module, movement, avatar binding
```

- **Bootstrap root** owns runtime host, survival bootstrap, survival module, and debug overlay only.
- **Player root** owns placeholder geometry and the camera follow target child.
- **Main Camera + CinemachineBrain** provide rendering; **one Cinemachine Camera** provides prototype follow.
- **Survival vitals** remain service-driven via `CCS_ISurvivalVitalsService`; no references from vitals code to camera or player transforms.

### 5. Phase 1D Implementation Steps

1. Ensure the scene has a **Main Camera** with **CinemachineBrain** (default Cinemachine 3 scene setup is acceptable).
2. Add **one** `CinemachineCamera` for prototype follow (e.g. `CM_PrototypeFollow`).
3. Create a simple **player placeholder root** in the scene (e.g. `CCS_PlayerRoot`).
4. Add a follow target child on the player root: **`CCS_PlayerCameraTarget`** (empty transform at chest/head height is sufficient).
5. Configure the Cinemachine Camera **Follow** and **Look At** to `CCS_PlayerCameraTarget` (same target for minimal prototype is fine).
6. Tune a modest follow offset/distance so the player is visible and the debug overlay stays out of center view.
7. Keep bootstrap prefab/scene wiring minimal — no custom camera scripts required for Phase 1D.
8. Verify Play Mode: Game View renders, camera tracks target, survival service still registers on bootstrap root.

### 6. Initial Implementation Scope

**In scope**

- Main Camera + CinemachineBrain (fixes “No cameras rendering”)
- One Cinemachine Camera with follow/look-at on `CCS_PlayerCameraTarget`
- Simple placeholder player root + camera target child
- Survival overlay and diagnostics unchanged on bootstrap root

**Out of scope (Phase 1D)**

- Aim camera
- Camera mode switching
- Shoulder camera
- Combat camera
- Final character controller
- Final animation
- **Input Actions asset** (New Input System deferred until movement implementation)
- Combat, inventory, final HUD
- Networking / replication

### 7. Input Direction

Player movement and control will use the **Unity New Input System** when a later milestone adds locomotion. **Do not** create an Input Actions asset for Phase 1D unless actual movement is implemented in the same pass.

The placeholder player may remain static for prototype camera/survival validation.

### 8. Done Criteria

- [x] Game View renders a basic scene (no “No cameras rendering”)
- [x] Main Camera has **CinemachineBrain**
- [x] One **CinemachineCamera** follows/looks at **CCS_PlayerCameraTarget**
- [x] Player placeholder root is visible in the world
- [x] Camera framing is stable for prototype testing
- [x] Survival debug overlay remains readable and top-right
- [x] `CCS_ISurvivalVitalsService` still registers on bootstrap root (`Services=1`)
- [x] Diagnostics remain clean (no new error spam)
- [x] Scene remains simple and prototype-focused

### 9. Standalone Build Checkpoint

After Phase 1D **implementation** proves Cinemachine camera + player placeholder + survival overlay together in Play Mode, create a **standalone build** smoke test (**0.4.0-A** — completed). Phase 1E targets **0.4.0-B** after movement is implemented.

---

## Implementation Status (Phase 1E)

- Basic **CharacterController** movement on **`CCS_PlayerRoot`** via `CCS_SurvivalPrototypeCharacterController`
- New Input System asset: `Assets/CCS/Survival/Settings/Input/CCS_Survival_InputActions.inputactions` (`Gameplay` map: Move, Look, Sprint, Jump)
- **Mouse/keyboard** (WASD, arrows, Left Shift) and **gamepad** (left stick, left stick press) bindings present
- Camera-relative movement using **Main Camera** transform; world-relative fallback with one-time warning if unset
- **Look** action bound but not consumed (camera look deferred)
- **Jump** action bound; **disabled** on controller (`enableJump` off) for prototype
- Prototype movement test pad added in scene: **`CCS_PrototypeGround`** (40x40 plane near origin) + **`CCS_PrototypeStep`** collision step
- Movement/gravity validation support added so CharacterController settles against ground instead of falling indefinitely
- Prototype visual readability pass added for movement testing
- Yellow player marker and simple ground spatial reference added (prototype materials + lightweight grid line strips)
- Movement-space debug option added (`CameraRelative` / `WorldRelative`) on prototype controller
- Scene temporarily set to **WorldRelative** to verify straighter A/D lateral movement during Phase 1E testing
- Survival vitals debug log noise reduced (health-change logs now step-based at larger intervals, events/overlay unchanged)
- Dynamic glyph UI still deferred; **stamina** sprint gating delivered in Phase 1F.1
- Terrain system and final environment art remain deferred
- `PF_CCS_Survival_BootstrapRoot` unchanged; vitals remain on composition root
- Standalone build checkpoint **0.4.0-B** performed for movement validation outside Editor

### Phase 1E manual validation

1. Open `SCN_CCS_Survival_Bootstrap.unity` → Play Mode
2. **WASD** / arrows move capsule; **Left Shift** sprints; gravity keeps player grounded
3. Gamepad left stick moves if connected; left stick press sprints
4. CharacterController collides with **`CCS_PrototypeGround`** and **`CCS_PrototypeStep`** as expected
5. Camera follows via `CM_PrototypeFollow`; survival overlay top-right; console **`Services=1`**

---

## Implementation Status (Phase 1D)

- Cinemachine **3.1.6** prototype camera setup implemented in `SCN_CCS_Survival_Bootstrap.unity`
- **Main Camera** tagged `MainCamera` with **CinemachineBrain**, **Camera**, and **AudioListener**
- **CM_PrototypeFollow** (`CinemachineCamera`) with **CinemachineFollow** (offset behind/above) and **CinemachineHardLookAt**
- Follow / Look At target: **CCS_PlayerCameraTarget** (child of **CCS_PlayerRoot** at chest/head height)
- **CCS_PlayerRoot** at world origin with capsule **PlaceholderVisual** (collider removed)
- **Directional Light** added for prototype visibility
- `PF_CCS_Survival_BootstrapRoot` unchanged as composition root (module, overlay, diagnostics)
- Player movement, New Input System, and final character controller still deferred

### Phase 1D manual validation

1. Open `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity`
2. Enter Play Mode
3. Confirm Game View shows capsule + sky (no “No cameras rendering”)
4. Confirm **Main Camera** has **CinemachineBrain**; **CM_PrototypeFollow** targets **CCS_PlayerCameraTarget**
5. Confirm top-right survival overlay and console `Services=1`

---

## Implementation Status (Phase 1C)

- Survival tuning moved to `CCS_SurvivalVitalsProfile` ScriptableObject (`Runtime/Survival/Modules/CCS_SurvivalVitalsProfile.cs`)
- Default profile asset: `Assets/CCS/Survival/Settings/Survival/CCS_DefaultSurvivalVitalsProfile.asset`
- `CCS_SurvivalModule` references profile instead of per-component tuning fields
- Safe fallback tuning values used only when profile is missing (warning logged, no exceptions)
- Create menu: **Assets → Create → CCS → Survival → Survival Vitals Profile**

---

## Implementation Status (Phase 1A)

- Phase 1A runtime skeleton created
- Survival module/service/state added (`CCS_SurvivalModule`, `CCS_ISurvivalVitalsService`, `CCS_SurvivalState`)
- Temporary debug overlay added (`CCS_SurvivalDebugOverlay`)
- Final UI still deferred
- AI test harness still deferred

---

## Implementation Status (Phase 1B.1)

- `CCS_SurvivalModule` moved to bootstrap composition root (`PF_CCS_Survival_BootstrapRoot`) with `CCS_RuntimeHost`
- `CCS_ISurvivalVitalsService` registers on service registry when runtime host is initialized
- Diagnostics expected service count updated to 1
- Separate `CCS_Survival_TestHarness` scene object removed (module + overlay live on bootstrap prefab)

---

## Implementation Status (Phase 1B)

- Manual Play Mode validation setup added on bootstrap composition root (`PF_CCS_Survival_BootstrapRoot`)
- Debug overlay used only for temporary testing (top-right compact panel)
- Context menu debug helpers on `CCS_SurvivalModule` for damage, recovery, food/water, kill, respawn, exposure
- Fast drain/damage tuning via `CCS_DefaultSurvivalVitalsProfile` for short Play Mode validation cycles
- Final UI still deferred
- Standalone build testing not required yet

### Manual validation steps

1. Open `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity`
2. Enter Play Mode on `PF_CCS_Survival_BootstrapRoot` (composition root)
3. Confirm diagnostics log shows `Services=1`
4. Observe top-right overlay: hunger/thirst drain, health loss when depleted, death, then use **Debug/Respawn Player** on `CCS_SurvivalModule`
5. Optional: use other **Debug/** context menu actions on the module while playing

---

## Purpose

Phase 1 establishes the minimum player survival loop required to prove CCS Survival's foundational fantasy: the wilderness applies pressure, the player responds, and recovery or failure is mechanically legible.

This phase delivers the core vitals and consequence systems:

- hunger
- thirst
- health
- stamina
- temperature exposure
- injury-lite
- death and respawn foundation

This document is planning and engineering direction only. No gameplay code should be written from this file without a follow-up implementation pass.

---

## Phase Goal

The player should be able to:

- become hungry
- become thirsty
- lose health from starvation, dehydration, or exposure
- recover through food, water, warmth, or rest
- die
- respawn
- expose core survival state through events and debug UI later

Phase 1 success is measured by a playable survival pressure loop, not by content breadth or presentation polish.

---

## Design Rules

- **Grounded survival, not hardcore simulation:** Systems should feel believable and consequential without becoming micromanagement-heavy.
- **Pressure should be meaningful but not annoying:** Drain rates, damage thresholds, and recovery windows must be tunable and testable.
- **Survival should support frontier lifestyle gameplay:** Hunger, thirst, and exposure should encourage planning, homesteading, and return-to-town behavior.
- **Systems must be modular and multiplayer-conscious:** Authority boundaries, service contracts, and event-driven updates should avoid single-player-only shortcuts.
- **No MMO/networking implementation yet:** Architecture may reserve hooks, but replication and session sync are out of scope.
- **No advanced disease simulation in Phase 1:** Illness, infection chains, and long-term medical conditions are deferred.
- **No reputation-scaled death penalties yet:** Death handling is functional and neutral in Phase 1.

---

## Proposed Runtime Systems

The following are **proposed** script/class names. Final naming may be adjusted during implementation while preserving CCS conventions and module boundaries.

| Proposed Type | Role |
|---------------|------|
| `CCS_SurvivalModule` | Module installer/registration entry for survival vitals services |
| `CCS_ISurvivalVitalsService` | Service contract for reading/updating survival state (extends foundation `CCS_ISurvivalService`) |
| `CCS_SurvivalState` | Aggregate runtime state container for a survival authority |
| `CCS_SurvivalStat` | Generic stat model (current, min, max, drain/recovery modifiers) |
| `CCS_SurvivalVitals` | Hunger/thirst/health/stamina update orchestration |
| `CCS_SurvivalTemperatureState` | Body temperature and exposure pressure model |
| `CCS_SurvivalInjuryState` | Lightweight injury severity tracking |
| `CCS_SurvivalDamageSource` | Typed damage context (starvation, dehydration, exposure, etc.) |
| `CCS_SurvivalRespawnPoint` | Respawn location/provider contract |
| `CCS_SurvivalDebugOverlay` | Development-only state visualization and logging controls |

Implementation should align with existing Survival foundation patterns (`CCS_SurvivalBootstrap`, authority/avatar contracts, validation utilities, diagnostics constants).

---

## Core Stats

### Health

- **Purpose:** Primary life state; death occurs when health reaches the configured threshold.
- **Rough behavior:** Decreases from damage sources; increases from rest, food/water recovery hooks, and safe recovery states.
- **Rise/fall causes:** Starvation, dehydration, exposure, injury-lite, and future combat hooks (stub only in Phase 1).
- **Dangerous levels:** Low health reduces survivability margin; at or below death threshold, player enters death flow.

### Hunger

- **Purpose:** Long-horizon resource pressure encouraging food acquisition and planning.
- **Rough behavior:** Drains passively over time; restored by consumable food interactions.
- **Rise/fall causes:** Time-based drain, activity modifiers (optional), consumable restoration.
- **Dangerous levels:** High hunger applies escalating health drain or efficiency penalties.

### Thirst

- **Purpose:** Shorter-cycle pressure than hunger; reinforces water sourcing behavior.
- **Rough behavior:** Drains faster than hunger baseline; restored by drink interactions.
- **Rise/fall causes:** Time-based drain, heat/exertion modifiers (optional), consumable restoration.
- **Dangerous levels:** High thirst applies health drain and stamina penalties.

### Stamina

- **Purpose:** Short-term action economy for sprinting, hauling, and exertion.
- **Rough behavior:** Drains during exertion; recovers during rest/low activity.
- **Rise/fall causes:** Movement/exertion drain, rest recovery, hunger/thirst stress modifiers (optional).
- **Dangerous levels:** Low stamina limits action cadence; should not hard-lock player movement in Phase 1.

### Body Temperature

- **Purpose:** Represents thermal comfort relative to safe range.
- **Rough behavior:** Moves toward environmental pressure; recovers near warmth sources/shelter.
- **Rise/fall causes:** Ambient temperature, weather exposure, clothing/shelter modifiers (basic).
- **Dangerous levels:** Hypothermia/heat stress bands increase exposure damage and stamina drain.

### Exposure

- **Purpose:** Accumulated environmental danger when unprotected in harsh conditions.
- **Rough behavior:** Builds while outside safe thermal zones; decays in shelter/warmth.
- **Rise/fall causes:** Storms, cold/heat extremes, lack of shelter, wet/cold stacking (basic).
- **Dangerous levels:** High exposure increases health drain rate and temperature instability.

### Injury Severity

- **Purpose:** Lightweight injury state for future medical depth without full trauma simulation.
- **Rough behavior:** Discrete severity tiers (none, minor, moderate, severe-lite).
- **Rise/fall causes:** Falls, animal attacks (stub), environmental hazards (stub).
- **Dangerous levels:** Higher severity applies health drain and reduced recovery efficiency.

---

## Events

Phase 1 should publish state changes through event-driven updates for UI, debug tooling, and future systems.

Expected event style (final names must follow CCS event conventions during implementation):

- `OnSurvivalStateChanged`
- `OnHealthChanged`
- `OnHungerChanged`
- `OnThirstChanged`
- `OnStaminaChanged`
- `OnTemperatureChanged`
- `OnPlayerDied`
- `OnPlayerRespawned`

Event payloads should include:

- authority/player identifier
- previous value
- new value
- change source (`CCS_SurvivalDamageSource` or recovery source)
- timestamp/tick context for test validation

---

## Data Flow

```text
Character Controller / Player Input
        ↓
CCS_SurvivalModule (registered service)
        ↓
CCS_SurvivalVitals + Temperature/Injury sub-states
        ↓
Damage / Recovery Processing
        ↓
Survival Events (state change, death, respawn)
        ↓
UI / Debug Overlay / Test Harness Observers
```

Update model recommendation:

- Fixed-tick or frame update pass owned by survival service (not scattered MonoBehaviour logic)
- Deterministic update order documented in module installer
- No direct UI writes from vitals internals

---

## Inspector / Configuration Direction

Tuning values must be serialized and inspectable (ScriptableObject profile and/or module config asset preferred).

Phase 1 configuration targets:

| Setting | Purpose |
|---------|---------|
| hunger drain rate | Baseline hunger pressure |
| thirst drain rate | Baseline thirst pressure |
| starvation damage rate | Health loss at critical hunger |
| dehydration damage rate | Health loss at critical thirst |
| exposure damage rate | Health loss under high exposure |
| stamina drain/recovery | Exertion economy tuning |
| death health threshold | Health value that triggers death |
| respawn delay | Time before respawn execution |
| debug logging toggle | Verbose survival state diagnostics |

Configuration should support per-profile overrides for rapid playtest iteration.

---

## Testing Expectations

Manual and diagnostic validation for Phase 1:

- player can starve
- player can dehydrate
- player can take exposure damage
- player can die
- player can respawn
- debug logs clearly show state changes
- system can be tested without final UI

Recommended test modes:

- deterministic drain profiles (fast-forward tuning)
- forced damage/recovery commands via debug overlay
- death/respawn cycle repeatability checks

---

## AI Test Harness Notes

Phase 1 does **not** implement AI agents, but systems must be designed so later harnesses can validate survival loops automatically.

Future AI test agents should be able to:

- consume food
- consume water
- stand in exposure
- rest/recover
- die/respawn
- report test pass/fail with structured logs

Harness requirements for Phase 1 design:

- service-level read/write APIs (not UI-only controls)
- explicit event subscriptions for state transitions
- deterministic config profiles for repeatable scenarios
- test assertions based on stat thresholds and event ordering

---

## Phase 1 Done Criteria

Phase 1 is done when the survival core can run in play mode and prove the player can suffer, recover, die, and respawn using configurable survival values.

Minimum acceptance checklist:

- [ ] Hunger and thirst drain over time
- [ ] Critical hunger/thirst apply health consequences
- [ ] Exposure can damage the player in harsh conditions
- [ ] Recovery paths (food, water, warmth/rest) function
- [ ] Death triggers at configured threshold
- [ ] Respawn restores player to valid state/location
- [ ] Survival events fire for major state transitions
- [ ] Debug visibility exists for validation without final UI

---

## Deferred Systems

The following are explicitly out of Phase 1 scope:

- advanced disease
- deep medical treatment
- limb injuries
- reputation-based death consequences
- advanced weather simulation
- final UI
- multiplayer replication
- economy interaction
- food spoilage
- cooking depth

---

## Related Documents

| Document | Path |
|----------|------|
| Prototype Roadmap | [CCS_Survival_Prototype_Roadmap.md](CCS_Survival_Prototype_Roadmap.md) |
| Gameplay Loop Specification | [../../Documentation/Gameplay/CCS_Survival_Gameplay_Loop_Specification.md](../../Documentation/Gameplay/CCS_Survival_Gameplay_Loop_Specification.md) |
| Gameplay Systems Breakdown | [../../Documentation/Gameplay/CCS_Survival_Gameplay_Systems_Breakdown.md](../../Documentation/Gameplay/CCS_Survival_Gameplay_Systems_Breakdown.md) |
| Gameplay Constitution | [../../Documentation/Gameplay/CCS_Survival_Gameplay_Constitution.md](../../Documentation/Gameplay/CCS_Survival_Gameplay_Constitution.md) |

---

## Implementation Notes (Engineering Guardrails)

- Register survival services through module installer flow; avoid global singletons.
- Keep survival logic in `CCS.Survival.Runtime` (or future `ccs.survival.vitals` module assembly) without modifying Core Platform behavior.
- Use authority IDs for state ownership to preserve multiplayer-conscious boundaries.
- Prefer profile-driven tuning over hard-coded constants.
- Add diagnostics constants for module validation and smoke-test visibility.
