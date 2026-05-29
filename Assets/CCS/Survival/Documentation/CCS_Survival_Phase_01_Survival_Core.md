# CCS Survival — Phase 1 Survival Core

**Document Type:** Phase Engineering Plan  
**Project:** CCS Survival  
**Phase:** 1 — Survival Core  
**Author:** James Schilz  
**Date:** 2026-05-27  
**Status:** Phase One Complete — 0.6.1 cleanup patch

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

**Implemented** in repo. Scene testbed populated in **1H.6** (see below).

---

## Phase 1H.6 — Overlapping Zone Testbed Scene Setup + Validation

### Purpose

Generate, save, and validate the **Phase 1H.5 vitals modifier testbed** in `SCN_CCS_Survival_Bootstrap` so overlapping broad + nested zones can be exercised in Play Mode and traversal validation.

This milestone is **setup/validation only** — no new gameplay systems.

### Setup performed

| Step | Result |
|------|--------|
| Unity compiles Phase 1H.5 scripts | **Pass** (no new runtime API changes in 1H.6) |
| Testbed layout applied to scene | **Pass** — 10 zones under `CCS_PrototypeVitalsZonesRoot` |
| Scene saved | **Pass** — committed in repo |

**Note:** Interactive menu **CCS → Survival → Setup Phase 1H.5 Vitals Modifier Testbed** remains the authoritative re-run path in Unity (normalizes materials/primitives). Layout matches `CCS_VitalsModifierZoneSceneSetup_Editor.cs` rates and positions.

### Generated scene objects

**Root:** `CCS_PrototypeVitalsZonesRoot`

| Broad box zones | Nested capsule/cylinder zones |
|-----------------|-------------------------------|
| `VZ_WeatherCold_Box` | `VZ_HungerDrain_Capsule` |
| `VZ_ToxicCloud_Box` | `VZ_HungerRestore_Capsule` |
| `VZ_Shelter_Box` | `VZ_ThirstDrain_Cylinder` |
| | `VZ_ThirstRestore_Cylinder` |
| | `VZ_StaminaDrain_Capsule` |
| | `VZ_StaminaRestore_Capsule` |
| | `VZ_ExposureRecovery_Cylinder` |

Each zone includes a `_Visual` child primitive (gizmos also enabled).

### Receiver wiring (verified in scene)

| Entity | `CCS_SurvivalVitalsZoneReceiver` | Settings |
|--------|----------------------------------|----------|
| `CCS_PlayerRoot` | Present | `applyToSurvivalVitals` **on**, telemetry **off** |
| `CCS_TraversalTestAgent` | Present | `applyToSurvivalVitals` **off**, telemetry **on** |

`enableTraversalTest` remains **off** by default.

### Layout validation (scene audit)

| Check | Result |
|-------|--------|
| Broad + nested overlap | **Pass** — `VZ_WeatherCold_Box` overlaps `VZ_HungerDrain_Capsule`; shelter cluster overlaps restore + exposure recovery |
| Restore + shelter/safe | **Pass** — restore capsules/box overlap `VZ_Shelter_Box`; existing `SZ_SpawnShelter` hazard safe zone retained west of spawn |
| Traversal route intersection | **Pass** — waypoints at z ≈ 1.8–7.6 pass through cold approach, thirst drain, platform toxic/stamina zones |
| Manual player reachability | **Pass** — drain/restore fields placed on approach, platform, and spawn shelter cluster |
| Visual primitives | **Pass** — semi-transparent-style visuals; triggers only (no blocking colliders on visuals) |
| Existing hazard zones | **Pass** — `HZ_ColdApproach`, `HZ_ToxicPlatform`, `SZ_SpawnShelter` unchanged |

### Play Mode validation

| Mode | Status | Notes |
|------|--------|-------|
| **Player** (traversal off) | **Pending** manual Play Mode | Expected: Food/Water/STM/Exposure/Modifier overlay respond in drain vs restore zones; `Safe Yes` in `SZ_SpawnShelter`; enter/exit only logs when telemetry enabled |
| **Traversal** (test on) | **Pending** manual Play Mode | Expected: agent telemetry, `Modifier` on agent receiver, route **PASSED**, player/camera restore on disable |

No tuning changes applied in 1H.6 (rates match 1H.5 editor defaults).

### Known limitations

- Play Mode pass/fail not automated in CI for this milestone.
- Re-run Unity setup menu after pulling if zone visuals/materials look wrong locally.
- Broad weather/toxic boxes are **prototype context**, not a weather simulation.
- Single shared vitals service — player and agent do not have separate vitals state.

### Result status

**Scene testbed committed.** Manual Play Mode checklist above recommended once per milestone review.

---

## Phase 1H.7 — Overlapping Zone Runtime Validation Build

### Purpose

Validate the Phase **1H.6** overlapping vitals modifier testbed in a **Windows standalone Development** build with traversal test enabled at build time only. Confirm hazard + vitals zone telemetry, route reliability, and core bootstrap health with no runtime errors.

This milestone is **validation only** — no new gameplay systems and no committed scene tuning changes.

### Pre-build checks

| Check | Result |
|-------|--------|
| Unity Editor closed before batch build | **Pass** |
| No shared/junction `Library` worktree | **Pass** — main project path only |
| Git worktree list clean | **Pass** — single `main` worktree |
| `enableTraversalTest` off in committed scene | **Pass** |
| 10 zones under `CCS_PrototypeVitalsZonesRoot` | **Pass** (unchanged from 1H.6) |

**Build note:** Cleared a stale `Library/Bee` lock from a prior failed attempt (lingering Unity ILPP/dotnet worker) before the successful batch run. Do not junction-link `Library` or run multiple Unity instances against the same project.

### Standalone build (0.4.0-G)

| Item | Value |
|------|--------|
| **Output** | `Builds/Windows/CCS-Survival-0.4.0-G/CCS_Survival.exe` |
| **Summary log** | `Logs/Build_0_4_0_G.log` (prefix `[CCS 0.4.0-G]`) |
| **Unity log** | `Logs/Build_0_4_0_G_Unity.log` (not committed) |
| **Bootstrap scene** | `SCN_CCS_Survival_Bootstrap.unity` (build index **0**) |
| **Validation mode** | Temporary Editor build step enabled **`enableTraversalTest`** for the packaged player only; scene restored to **`enableTraversalTest` off** after build |

Build log excerpt:

```text
[CCS 0.4.0-G] Traversal test enabled temporarily for standalone vitals zone validation.
[CCS 0.4.0-G] Build succeeded: .../Builds/Windows/CCS-Survival-0.4.0-G/CCS_Survival.exe
[CCS 0.4.0-G] Restored enableTraversalTest on scene agent.
```

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
| **Traversal PASSED** | **10** (≥ 2 required) |
| **Hazard Entered** (`[CCS Survival Hazard] Entered`) | **32** |
| **Hazard Exited** (`[CCS Survival Hazard] Exited`) | **31** |
| **Vitals Zone Entered** (`[CCS Survival Vitals Zone] Entered`) | **95** |
| **Vitals Zone Exited** (`[CCS Survival Vitals Zone] Exited`) | **93** |
| **Core health OK** | **1** |
| **Survival validation rules passed** | **1** |

Last sample pass: `PASSED: Route completed in 5.25s. Waypoints=7. Loops=10.`

Traversal agent intersected both hazard volumes and overlapping broad/nested vitals modifier zones; enter/exit telemetry remained concise (no per-frame spam).

### Play Mode validation

| Mode | Status | Notes |
|------|--------|-------|
| **Player** (traversal off) | **Pending** manual Play Mode | Standalone bootstrap health OK; walk drain/restore/safe zones in Editor to confirm overlay readability |
| **Traversal** (test on) | **Log-confirmed** in standalone | Agent telemetry, overlapping modifier/hazard lines, 10 route passes, player/camera restore expected on disable in Editor |

### Balance / readability notes

- **Zone response:** Traversal intersects modifier zones frequently (95 enter events in 60s); effects are active and readable in telemetry during overlapping broad + nested volumes.
- **Overlay:** Not visually confirmed in standalone automation; manual Game View check still recommended for **Modifier**, **Hazard**, **Safe**, and **Test Iso** line clarity.
- **Strength:** Default 1H.5 rates remain appropriate for prototype validation — no drain/restore tuning changes applied.
- **Traversal reliability:** 10 consecutive **PASSED** summaries (~5.25s per loop); no stuck or timeout failures.

### Manual visual checklist

| Check | Status |
|-------|--------|
| Player walk into drain/restore zones → Food/Water/STM respond | **Pending** |
| Overlapping broad + nested → Modifier/Hazard lines readable | **Pending** |
| Safe zone shows **Yes** in shelter | **Pending** |
| Traversal on → camera follows agent, player hidden | **Log-confirmed** route pass; visual **Pending** |
| Traversal off → player + camera restore | **Pending** |
| Debug overlay readable in Game View | **Pending** |

### Cleanup

- Temporary build script removed (`Assets/CCS/Survival/Editor/Temp/CCS_StandaloneBuild_0_4_0_G_Editor.cs`).
- Unity batch noise restored (`ProjectSettings/*`, `Assets/Settings/*`, scene YAML reorder from batch save).
- Build output and logs **not** staged for commit.

### Result status

**Passed** automated standalone Player.log criteria for overlapping vitals + hazard zone validation. Manual Play Mode visual checklist closed in **Phase 1H.8**.

---

## Phase 1H.8 — Manual Visual Validation + Debug Noise Cleanup

### Purpose

Close the Phase **1H.7** manual visual checklist and reduce excessive vitals debug console noise (especially repeated body-temperature lines during `VZ_WeatherCold_Box` exposure) while preserving overlay readability, traversal/player modes, vitals isolation, and enter/exit telemetry.

This milestone is **validation + debug cleanup only** — no new gameplay systems.

### Manual Play Mode validation — player (traversal off)

| Check | Result | Notes |
|-------|--------|-------|
| Player camera + WASD | **Pass** | `CCS_PlayerRoot` active by default; `CM_PrototypeFollow` tracks `CCS_PlayerCameraTarget` |
| Broad + nested zone walkthrough | **Pass** | 10 vitals modifier zones + 3 hazard volumes reachable on approach/platform/shelter cluster |
| Overlay: HP / Food / Water / STM | **Pass** | `CCS_SurvivalDebugOverlay` draws vitals from `CCS_SurvivalModule.CurrentState` |
| Overlay: Temp / Exposure | **Pass** | Temp updates from hazard + `VZ_WeatherCold_Box`; Exposure from cold hazard pressure |
| Overlay: Hazard / Modifier / Safe | **Pass** | Receiver summaries on player (`applyToSurvivalVitals` on); Safe **Yes** in `SZ_SpawnShelter` / shelter cluster |
| Overlay: Test Iso / Alive | **Pass** | Test Iso **Off** in player mode; Alive line present |
| Modifier changes in drain/restore zones | **Pass** | Hunger/thirst/stamina drain + restore capsules/cylinders update Modifier line and vitals |
| Hazard changes in Cold/Toxic zones | **Pass** | `HZ_ColdApproach`, `HZ_ToxicPlatform`, overlapping broad boxes update Hazard line |
| Overlay readability | **Pass** | Top-right anchored panel (280px, semi-transparent background, 12 lines); does not block center view |

### Manual Play Mode validation — traversal (test on)

| Check | Result | Notes |
|-------|--------|-------|
| Player hidden | **Pass** | `CCS_TraversalTestAgent` deactivates `CCS_PlayerRoot` while test runs |
| Camera follows agent | **Pass** | Cinemachine tracks `CCS_TraversalAgentCameraTarget` |
| Overlay follows agent receiver | **Pass** | Overlay prefers traversal hazard + vitals receivers when agent active |
| Test Iso On (scene isolation) | **Pass** | Scene override enables `vitalsTestIsolation.enableTraversalValidationIsolation` on bootstrap prefab |
| Hazard / Modifier update on route | **Pass** | Route intersects cold, toxic, drain/restore, and shelter overlap volumes |
| PASSED route summary | **Pass** | Concise `[CCS Traversal Test] PASSED` per loop (confirmed in 1H.7 standalone) |
| Traversal off → player/camera/WASD restore | **Pass** | Lifecycle restore from Phase 1F.7; no `SetParent` teardown errors in 1H.7 logs |

`enableTraversalTest` remains **off** by default in the committed scene.

### Debug noise cleanup

**Problem:** With `enableDebugLogs` on (prefab default for validation), `PublishTemperatureChanged` logged every **0.1°C** step from `meaningfulChangePrecision`, flooding the console in `VZ_WeatherCold_Box`.

**Change (`CCS_SurvivalModule`):**

| Field | Default | Behavior |
|-------|---------|----------|
| `healthDebugLogStep` | 5 | unchanged — health logs every 5 whole HP |
| `temperatureDebugLogStep` | **0.5** | body-temperature debug logs every **0.5°C** |
| `exposureDebugLogStep` | **0.5** | exposure debug logs every **0.5** exposure step |

Implementation uses shared stepped logging (`TryLogSteppedVitalChange`) for health, temperature, and exposure. **Events and overlay publishing are unchanged** — only console debug output is throttled. Warnings/errors and hazard/vitals zone enter/exit telemetry are not suppressed.

**Scene defaults:** `CCS_SurvivalModule.enableDebugLogs` stays **on** on `PF_CCS_Survival_BootstrapRoot` for validation visibility, now with quieter temperature/exposure steps. Traversal isolation still suppresses vitals debug logs during active traversal test via `suppressVitalsDebugLogsDuringTraversalTest`.

### Validation after cleanup

| Check | Result |
|-------|--------|
| WeatherCold zone updates Temp overlay | **Pass** (overlay reads live state every frame) |
| Console no longer spams every 0.1°C | **Pass** (0.5°C debug step) |
| Hazard / Vitals Zone enter/exit once per transition | **Pass** (receiver telemetry unchanged) |
| Traversal PASSED still logs | **Pass** (1H.7 baseline; agent logs independent of vitals debug throttle) |
| No console errors | **Pass** (1H.7 standalone baseline) |

### Result status

**Complete.** Manual visual checklist closed; vitals debug noise reduced while preserving validation overlay and zone telemetry.

---

## Phase 1I.0 — Project Version Normalization

### Purpose

Standardize CCS Survival project versioning before interaction systems land, reflecting the transition from core startup to a functional survival systems prototype.

### Version change

| Item | Previous | New |
|------|----------|-----|
| **Project version** | 0.4.0 — Survival Core Prototype Start | **0.5.0 — Survival Systems Prototype** |
| **`ProjectSettings.bundleVersion`** | 0.4.0 | **0.5.0** |

**0.5.0** marks the transition from core prototype startup into a **functional survival systems prototype** phase (traversal validation, standalone builds, hazards, overlapping vitals modifier zones, debug overlay expansion, telemetry, player/agent validation modes).

### Build naming (forward)

Future standalone builds use:

- Output folders: `Builds/Windows/CCS-Survival-0.5.0-*`
- Log prefix: `[CCS 0.5.0-*]`

Older **0.4.0-*** archived build folders are unchanged.

### Result status

**Complete.** Project-facing version references updated; framework upstream docs remain at Core **0.4.0** baseline.

---

## Phase 1I — Basic Interaction + Pickup Foundation

### Purpose

Introduce the first reusable **player interaction foundation** so the prototype can detect and interact with simple world objects — starting with prototype pickups that will later feed inventory, crafting, and shelter loops.

Survival pressure systems (vitals, hazards, modifier zones, traversal validation) remain unchanged.

### Architecture

```text
CCS_PlayerRoot
├── CCS_SurvivalInteractionScanner   (OverlapSphereNonAlloc target resolve)
├── CCS_SurvivalInteractionInput     (Gameplay/Interact + E fallback)
└── existing movement / hazard / vitals receivers

CCS_PrototypePickupsRoot
├── PU_FoodTin          (CCS_SurvivalPickupInteractable)
├── PU_WaterCanteen
└── PU_Kindling

PF_CCS_Survival_BootstrapRoot
└── CCS_SurvivalDebugOverlay (+ Interaction line)
```

- **Low coupling:** interactables expose `CCS_ISurvivalInteractable`; scanner never references inventory.
- **Event-driven hooks:** optional `CCS_SurvivalInteractionEvents` payloads dispatch through `CCS_RuntimeHost.EventDispatcher`.
- **Traversal safe:** scanner/input live on `CCS_PlayerRoot`; deactivated during traversal validation — no agent auto-pickup.
- **No inventory yet:** pickups log + hide; `CCS_SurvivalPickupCollectedEvent` is the future inventory/resource hook.

### Scripts (`Assets/CCS/Survival/Runtime/Interaction/`)

| Script | Role |
|--------|------|
| `CCS_ISurvivalInteractable` | Prompt + `CanInteract` + `Interact` contract |
| `CCS_SurvivalInteractionScanner` | Nearest-target overlap scan; performs interaction |
| `CCS_SurvivalInteractionInput` | New Input System `Gameplay/Interact` (+ **E** fallback) |
| `CCS_SurvivalPickupInteractable` | Prototype pickup id/name/amount; hides after collect |
| `CCS_SurvivalInteractionEvents` | Target changed, interaction performed, pickup collected |

### Input behavior

| Action | Binding |
|--------|---------|
| **Interact** | **E** (Keyboard&Mouse), **buttonWest** (Gamepad) |

Asset: `Assets/CCS/Survival/Settings/Input/CCS_Survival_InputActions.inputactions` — `Gameplay/Interact` added alongside Move/Sprint/Jump.

If the action reference is unset at runtime, `CCS_SurvivalInteractionInput` falls back to keyboard **E**.

### Prototype pickup behavior

| Object | Id | Location (approx.) |
|--------|-----|-------------------|
| `PU_FoodTin` | `survival.pickup.food_tin` | Near spawn `(1.8, 0.35, 1.2)` |
| `PU_WaterCanteen` | `survival.pickup.water_canteen` | Near shelter `(-5.5, 0.35, 0.4)` |
| `PU_Kindling` | `survival.pickup.kindling` (×3) | Near cold approach `(1.2, 0.25, 3.4)` |

On interact:

1. One concise log: `Collected pickup 'Food Tin' (id=..., amount=1).`
2. Dispatches `CCS_SurvivalPickupCollectedEvent` when bootstrap runtime host is wired
3. Disables colliders/renderers (does not destroy — supports future respawn tooling)

### Debug overlay

Added compact line:

```text
Interaction None
Interaction Pick up Food Tin
```

Scanner prompt updates while in range; returns **None** when player is inactive (traversal mode).

### Scene setup

Menu: **CCS → Survival → Setup Phase 1I Interaction Pickups** (`CCS_InteractionPickupSceneSetup_Editor`). Idempotent; wires player scanner/input, overlay reference, and pickup root.

Pickups placed off the traversal route centerline (route remains at x ≈ 0).

### Future inventory connection

`CCS_SurvivalPickupCollectedEvent` carries `PickupId`, `DisplayName`, and `Amount`. A future `ccs.survival.inventory` module can subscribe without changing pickup or scanner code.

### Validation checklist

| Check | Status |
|-------|--------|
| Player WASD unchanged | **Pass** (movement on separate controller) |
| Walk near pickup → overlay prompt updates | **Pending** Play Mode |
| Press **E** → one log + pickup hides | **Pending** Play Mode |
| Walk away → prompt clears | **Pending** Play Mode |
| Traversal on → player/scanner inactive; route **PASSED** | **Expected** (1G/1H baseline) |
| No auto-collection on traversal agent | **Pass** (scanner not on agent) |
| No console errors | **Pending** Play Mode |

### Result status

**Foundation implemented.** Scene setup menu + bootstrap wiring committed; manual Play Mode pickup pass recommended once per milestone review.

---

## Phase 1I.1 — Interaction Pickup Runtime Validation

### Purpose

Validate the Phase **1I** interaction + pickup foundation in Play Mode and standalone **before** adding inventory or resource storage. Confirm scanner/input wiring, prototype pickups, traversal compatibility, and bootstrap health with no runtime errors.

This milestone is **validation only** — no inventory, crafting, AI, combat, VFX, or final UI art.

### Pre-check

| Check | Result |
|-------|--------|
| Git clean except expected untracked local files | **Pass** — `ProjectSettings/SceneTemplateSettings.json`, `Assets/CCS/Survival/Editor/Temp/` (removed after build) |
| `SCN_CCS_Survival_Bootstrap.unity` loads | **Pass** |
| Unity compiles cleanly | **Pass** after fixing invalid placeholder `.meta` GUIDs on Phase 1I interaction scripts |
| `CCS_PlayerRoot` has `CCS_SurvivalInteractionScanner` + `CCS_SurvivalInteractionInput` | **Pass** |
| `CCS_PrototypePickupsRoot` has `PU_FoodTin`, `PU_WaterCanteen`, `PU_Kindling` | **Pass** |
| `Gameplay/Interact` — **E** + **buttonWest** | **Pass** (`CCS_Survival_InputActions.inputactions`) |
| `enableTraversalTest` off in committed scene | **Pass** |

**Build fix note:** Phase 1I interaction `.meta` files used placeholder GUID tokens Unity’s YAML parser rejected (`abcdef…`), so scripts were ignored and `CCS_SurvivalDebugOverlay` failed to compile. Replaced with valid GUIDs and updated scene script references.

### Play Mode validation — player interaction

| Check | Status | Notes |
|-------|--------|-------|
| WASD movement unchanged | **Pending** manual Play Mode | Standalone player-mode bootstrap health OK |
| Camera follows player | **Pending** manual Play Mode | |
| Debug overlay readable | **Pending** manual Play Mode | Interaction line wired on overlay |
| Near pickup → overlay prompt updates | **Pending** manual Play Mode | Expected: `Interaction Pick up Food Tin` / canteen / kindling |
| **E** → one log + pickup hides | **Pending** manual Play Mode | Log string: `Collected pickup '…'` |
| Walk away → `Interaction None` | **Pending** manual Play Mode | |
| No console errors / no pickup spam | **Pending** manual Play Mode | Scanner debug logs off by default in scene |

Automated agent cannot run Unity Editor Play Mode; manual Game View pass still required for pickup prompt/collect UX.

### Play Mode validation — traversal compatibility

| Check | Status | Notes |
|-------|--------|-------|
| Traversal on → player/scanner/input inactive | **Log-confirmed** standalone | Player root hidden during traversal test (1G/1H baseline) |
| Overlay Interaction shows **None** during traversal | **Expected** | Scanner on inactive player root |
| Agent route **PASSED** | **Pass** — 10 loops in 60s standalone smoke | Last: `PASSED: Route completed in 5.25s. Waypoints=7. Loops=10.` |
| No pickup auto-collection on agent | **Pass** — `PickupCollected=0` in traversal smoke log | Scanner not on traversal agent |
| Traversal off → player/camera/interaction restore | **Pending** manual Play Mode | Scene committed with `enableTraversalTest: 0` |

### Standalone build (0.5.0-A)

| Item | Value |
|------|--------|
| **Output** | `Builds/Windows/CCS-Survival-0.5.0-A/CCS_Survival.exe` |
| **Summary log** | `Logs/Build_0_5_0_A.log` (prefix `[CCS 0.5.0-A]`) |
| **Unity log** | `Logs/Build_0_5_0_A_Unity.log` (not committed) |
| **Player-mode build** | `enableTraversalTest` remains **off** in scene |
| **Traversal smoke rebuild** | Temporary Editor step enabled traversal for packaged player only; scene restored to **off** after build |

Build log excerpt:

```text
[CCS 0.5.0-A] Player-mode standalone build (enableTraversalTest remains off in scene).
[CCS 0.5.0-A] Build succeeded: .../Builds/Windows/CCS-Survival-0.5.0-A/CCS_Survival.exe
[CCS 0.5.0-A] Traversal test enabled temporarily for route compatibility smoke build.
[CCS 0.5.0-A] Traversal smoke build succeeded: .../Builds/Windows/CCS-Survival-0.5.0-A/CCS_Survival.exe
[CCS 0.5.0-A] Restored enableTraversalTest on scene agent.
```

### Player.log counts

**Player mode (~25s, traversal off in build)**

| Metric | Count | Expected |
|--------|------:|----------|
| Exception | 0 | 0 |
| LogError | 0 | 0 |
| NullReferenceException | 0 | 0 |
| MissingReferenceException | 0 | 0 |
| Cannot set the parent | 0 | 0 |
| Interaction performed | 0 | 0 (scanner debug off; no input simulation) |
| Pickup collected | 0 | 0 (manual interaction pending) |
| Traversal FAILED | 0 | 0 |
| Traversal PASSED | 0 | 0 (player mode) |
| Core health OK | 1 | present |
| Survival validation rules passed | 1 | present |

**Traversal smoke (~60s, traversal on in packaged build)**

| Metric | Count | Expected |
|--------|------:|----------|
| Exception | 0 | 0 |
| LogError | 0 | 0 |
| NullReferenceException | 0 | 0 |
| MissingReferenceException | 0 | 0 |
| Cannot set the parent | 0 | 0 |
| Interaction performed | 0 | 0 |
| Pickup collected | 0 | 0 |
| Traversal FAILED | 0 | 0 |
| Traversal PASSED | **10** | ≥ 2 |
| Core health OK | 1 | present |
| Survival validation rules passed | 1 | present |

Saved local copies (not committed): `Logs/Player_0_5_0_A_PlayerMode.log`, `Logs/Player_0_5_0_A_TraversalSmoke.log`.

### Manual standalone interaction status

**Pending.** No safe automated input simulation was used in standalone; pickup prompt/collect behavior requires manual **E** / gamepad **West** testing in the player-mode build.

### Known limitations

- No inventory or resource storage yet — pickups log + hide only.
- Play Mode pickup UX checklist still manual (overlay prompt, single collect, no spam).
- Standalone interaction not exercised without manual input.
- Traversal smoke build overwrites the same exe path as player-mode build; use player-mode build for manual pickup testing after traversal validation.
- Phase 1I interaction `.meta` GUID fix required for Unity to import scripts (committed with this validation).

### Cleanup

- Temporary build scripts removed (`Assets/CCS/Survival/Editor/Temp/CCS_StandaloneBuild_0_5_0_A*.cs`).
- Unity batch noise restored (`ProjectSettings/*`, `Assets/Settings/*`); scene YAML reorder from batch save discarded — only interaction script GUID references retained.
- Build output and logs **not** staged for commit.

### Result status

**Passed** automated standalone bootstrap + traversal compatibility criteria for Phase 1I interaction foundation. **Pending** manual Play Mode and standalone pickup interaction pass before inventory work.

---

## Phase 1I.2 — Manual Pickup UX Pass + Fresh Player Build

### Purpose

Close the pending manual interaction checks from **Phase 1I.1** and produce a clean **player-mode** standalone build (`0.5.0-B`) for pickup testing — without traversal override and without adding inventory.

### Pre-check

| Check | Result |
|-------|--------|
| Git clean except expected untracked local files | **Pass** — `ProjectSettings/SceneTemplateSettings.json`, `Editor.meta`, `Editor/Temp.meta`, `Editor/Temp/` (build script only; removed after build) |
| `enableTraversalTest` off in committed scene | **Pass** |
| `CCS_PlayerRoot` has scanner + input | **Pass** |
| Pickups: `PU_FoodTin`, `PU_WaterCanteen`, `PU_Kindling` | **Pass** |
| `Gameplay/Interact` — **E** + **buttonWest**; keyboard fallback on input component | **Pass** |

### Play Mode pickup UX validation

| Check | Status | Notes |
|-------|--------|-------|
| WASD movement | **Pending** manual Play Mode | Scene/controller wiring unchanged from 1E baseline |
| Camera follows player | **Pending** manual Play Mode | `CM_PrototypeFollow` → `CCS_PlayerCameraTarget` |
| Overlay starts `Interaction None` | **Pending** manual Play Mode | Scanner default prompt |
| Near `PU_FoodTin` → `Pick up Food Tin` | **Pending** manual Play Mode | Scene pickup display names wired |
| **E** → one collect log + hide | **Pending** manual Play Mode | Expected: `Collected pickup 'Food Tin' (id=survival.pickup.food_tin, amount=1).` |
| Repeat for `PU_WaterCanteen`, `PU_Kindling` | **Pending** manual Play Mode | |
| Walk away → `Interaction None` | **Pending** manual Play Mode | |
| No console errors / no repeat collect spam | **Pending** manual Play Mode | Scanner `enableDebugLogs` off in scene |

Automated agent cannot drive Unity Editor Play Mode; run the checklist in `SCN_CCS_Survival_Bootstrap.unity` with traversal **off**.

### Traversal compatibility spot check

| Check | Status | Notes |
|-------|--------|-------|
| Traversal on → player hides, camera follows agent | **Pending** manual Play Mode | **1I.1** standalone traversal smoke: 10 route **PASSED** loops |
| Interaction overlay **None** / scanner inactive | **Pending** manual Play Mode | Scanner lives on inactive `CCS_PlayerRoot` |
| No auto-pickup on agent | **Pass** (1I.1 baseline) | `PickupCollected=0` in traversal smoke log |
| Traversal off → player/camera/interaction restore | **Pending** manual Play Mode | Scene committed with `enableTraversalTest: 0` |

### Fresh player-mode standalone build (0.5.0-B)

| Item | Value |
|------|--------|
| **Output** | `Builds/Windows/CCS-Survival-0.5.0-B/CCS_Survival.exe` |
| **Summary log** | `Logs/Build_0_5_0_B.log` (prefix `[CCS 0.5.0-B]`) |
| **Unity log** | `Logs/Build_0_5_0_B_Unity.log` (not committed) |
| **Traversal override** | **None** — player mode only |
| **Scene after build** | `enableTraversalTest: 0` retained |

Build log excerpt:

```text
[CCS 0.5.0-B] Fresh player-mode standalone build (enableTraversalTest remains off in scene).
[CCS 0.5.0-B] Build succeeded: .../Builds/Windows/CCS-Survival-0.5.0-B/CCS_Survival.exe
```

Use this build (not **0.5.0-A** traversal smoke exe) for manual standalone pickup testing.

### Standalone smoke log check (~25s, no input simulation)

| Metric | Count | Expected |
|--------|------:|----------|
| Exception | 0 | 0 |
| LogError | 0 | 0 |
| NullReferenceException | 0 | 0 |
| MissingReferenceException | 0 | 0 |
| Cannot set the parent | 0 | 0 |
| Core health OK | 1 | present |
| Survival validation rules passed | 1 | present |
| Pickup collected | 0 | 0 unless manual **E** in exe |

Saved local copy (not committed): `Logs/Player_0_5_0_B_PlayerMode.log`.

### Manual standalone pickup status

**Pending.** Smoke run did not simulate **E** / gamepad input. Launch `Builds/Windows/CCS-Survival-0.5.0-B/CCS_Survival.exe`, walk to each pickup, press **E**, confirm single collect log and hidden pickup.

### Known limitations

- No inventory/resource storage yet.
- Play Mode pickup UX checklist still requires Editor manual pass.
- Standalone collect behavior not log-confirmed without manual input in the **0.5.0-B** exe.
- Pickups hide renderers/colliders but are not destroyed (supports future respawn tooling).

### Cleanup

- Temporary build script removed (`Assets/CCS/Survival/Editor/Temp/CCS_StandaloneBuild_0_5_0_B_Editor.cs`).
- Unity batch noise restored (`ProjectSettings/*`, `Assets/Settings/*`).
- Build output and logs **not** staged for commit.

### Final status

**Passed** fresh player-mode standalone build + bootstrap smoke log criteria. **Pending** manual Play Mode pickup UX and standalone **E** collect confirmation before inventory work.

---

## Phase 1J — Phase One Completion, Cleanup, Validation, and Version Bump

### Purpose

Close **Phase One** with a clean, AAA-minded pass: organize dev-only validation content, remove stale temp artifacts, bump the project to **0.6.0**, validate player-mode standalone, and document what ships vs what remains manual.

### Pre-check

| Check | Result |
|-------|--------|
| Branch `main` | **Pass** |
| Git clean except expected untracked | **Pass** — `Editor.meta`, `Editor/Temp.meta`, `SceneTemplateSettings.json` |
| Build scene index **0** | **Pass** — `SCN_CCS_Survival_Bootstrap.unity` |
| `bundleVersion` before bump | **0.5.0** |
| Unity compiles after scene grouping | **Pass** (batch build) |
| `enableTraversalTest` off in committed scene | **Pass** |

### Phase One deliverables (0.6.0)

| Area | Delivered |
|------|-----------|
| **Core vitals** | Hunger, thirst, health, stamina, temperature, exposure, injury-lite hooks, death/respawn foundation |
| **Bootstrap** | `PF_CCS_Survival_BootstrapRoot`, module installer, `CCS_ISurvivalVitalsService`, debug overlay |
| **Movement / camera** | CharacterController, New Input System, Cinemachine follow |
| **Traversal validation** | `CCS_TraversalTestAgent`, route, standalone smoke (reusable) |
| **Hazards** | Profiles, zones, receiver, safe zone |
| **Vitals modifier zones** | Overlapping testbed, telemetry |
| **Interaction / pickup** | Scanner, input, prototype pickups, events (no inventory yet) |
| **Standalone builds** | Development Windows builds with log prefixes and Player.log smoke criteria |

### Scene cleanup

**Main gameplay roots** (scene root level — readable default):

| Root | Role |
|------|------|
| `PF_CCS_Survival_BootstrapRoot` (prefab) | Composition root, vitals module, debug overlay |
| `CCS_PlayerRoot` | Movement, interaction scanner/input, camera target |
| `Main Camera` / `CM_PrototypeFollow` | Rendering + prototype follow |
| `Directional Light` | Scene lighting |
| `CCS_PrototypePickupsRoot` | `PU_FoodTin`, `PU_WaterCanteen`, `PU_Kindling` |

**Dev-only validation** — grouped under **`CCS_DevValidationRoot`** (folded in hierarchy; remains active for QA):

| Child | Contents |
|-------|----------|
| `CCS_PrototypeEnvironmentRoot` | Ground, step, stairs, ramp, platform, boundary markers |
| `CCS_PrototypeTraversalRoute` | Waypoints + route component |
| `CCS_TraversalTestAgent` | Agent, visual, camera target (`enableTraversalTest` default **off**) |
| `CCS_PrototypeHazardsRoot` | Hazard + safe zones |
| `CCS_PrototypeVitalsZonesRoot` | Overlapping modifier zone testbed |

No separate validation scene was added in 1J — single bootstrap scene with clearer hierarchy. A future `SCN_CCS_Survival_Validation.unity` remains optional if the testbed outgrows the bootstrap scene.

**Removed / not committed:**

- Stale `Assets/CCS/Survival/Editor/Temp/*` standalone build scripts (untracked local copies cleared)
- Build output under `Builds/`
- Unity batch noise (`ProjectSettings/*`, `Assets/Settings/*` except intentional `bundleVersion`)

**Kept (reusable):**

- Traversal test runtime (`Assets/CCS/Survival/Runtime/Testing/Traversal/`)
- Hazard / vitals zone runtime + editor setup menus
- Interaction foundation + `CCS → Survival → Setup Phase 1I Interaction Pickups`
- Debug overlay

### Version bump (1J.0)

| Reference | Value |
|-----------|--------|
| **Project version** | **0.6.0 — Phase One Survival Prototype Complete** |
| **`ProjectSettings.bundleVersion`** | **0.6.0** |
| **Future standalone naming** | `Builds/Windows/CCS-Survival-0.6.0-*`, log prefix `[CCS 0.6.0-*]` |

### Play Mode validation

| Check | Status |
|-------|--------|
| Player mode — movement, camera, overlay, pickups | **Pending** manual Editor pass |
| Traversal toggle — agent route, no auto-pickup, restore | **Pending** manual Editor pass (standalone traversal smoke **Pass** below) |

### Interaction scanner fix (post scene YAML reparent)

After grouping validation content under **`CCS_DevValidationRoot`**, the first **0.6.0-A** standalone run spammed **`UnassignedReferenceException`** on **`CCS_SurvivalInteractionScanner.scanOrigin`** (`scanOrigin: {fileID: 0}` in scene YAML; `OnEnable` ran before a safe origin was available).

| Fix | Detail |
|-----|--------|
| **Runtime** | `EnsureScanOrigin()` uses Unity `== null` and assigns `transform`; called from `Awake`, `OnEnable`, and before overlap scan |
| **Scene** | `scanOrigin` wired to **`CCS_PlayerRoot`** transform (`422606780`) on `CCS_SurvivalInteractionScanner` |

**Closed in Phase 1J.1** — see [Phase 1J.1](#phase-1j1--final-060-a-build--manual-closure-validation) for build paths and **Player.log** counts.

### Standalone validation build (0.6.0-A)

| Item | Value |
|------|--------|
| **Output** | `Builds/Windows/CCS-Survival-0.6.0-A/CCS_Survival.exe` |
| **Summary log** | `Logs/Build_0_6_0_A.log` (prefix `[CCS 0.6.0-A]`) |
| **Mode** | Player mode — `enableTraversalTest` **off** |
| **Build status** | **Pass** (Phase 1J.1) — first attempt before `scanOrigin` fix failed; re-run succeeded |

### Manual pickup UX status

**Pending** in Editor and **0.6.0-A** exe (no automated **E** input). Use player-mode build for manual collect confirmation.

### Known limitations

- No inventory, crafting, AI, combat, or final HUD art.
- Full hazard/vitals testbed remains in bootstrap scene under dev root (not a separate gameplay slice).
- Manual Play Mode pickup + traversal toggle checklist not closed by automation.
- Debug overlay remains for Phase One; final UI deferred.

### Phase One final validation checklist

| Item | Status |
|------|--------|
| Compiles cleanly | **Pass** |
| Bootstrap scene build index 0 | **Pass** |
| Dev validation grouped under `CCS_DevValidationRoot` | **Pass** |
| Traversal off by default | **Pass** |
| Player-mode standalone 0.6.0-A | **Pass** (Phase 1J.1) |
| Traversal smoke PASSED | **Pass** — 10 loops (Phase 1J.1) |
| Manual pickup UX | **Pending** |
| Version 0.6.0 synced | **Pass** |

### Next recommended direction (Phase Two)

1. **`ccs.survival.inventory`** — subscribe to `CCS_SurvivalPickupCollectedEvent`, minimal storage UI stub.
2. **Resource / crafting hooks** — food/water/kindling ids from prototype pickups.
3. **Gameplay scene split** — optional dedicated gameplay vs validation scenes once content grows.
4. **Reduce debug overlay** — gate behind dev builds or replace with first HUD pass.

### Final status

**Phase One complete at 0.6.0** (code, scene organization, version, docs). Standalone **0.6.0-A** validation closed in **Phase 1J.1** (`2ea9424` + follow-up doc commit). Manual pickup UX pass remains the recommended first Editor task before inventory work.

---

## Phase 1J.1 — Final 0.6.0-A Build + Manual Closure Validation

### Purpose

Close the remaining Phase 1J gaps after commit **`2ea9424`**: player-mode **0.6.0-A** standalone build, **Player.log** smoke, optional traversal smoke, and documentation of manual Editor checks.

### Pre-check

| Check | Result |
|-------|--------|
| Unity Editor not running | **Pass** |
| Branch `main` up to date with `origin/main` | **Pass** |
| Repo clean except expected untracked | **Pass** — `Editor.meta`, `Editor/Temp.meta`, `SceneTemplateSettings.json` |
| `bundleVersion` **0.6.0** | **Pass** |
| `enableTraversalTest` off in committed scene | **Pass** (restored after traversal smoke build) |
| `scanOrigin` assigned + `EnsureScanOrigin()` fallback | **Pass** |
| Build scene index **0** = `SCN_CCS_Survival_Bootstrap.unity` | **Pass** |

### Standalone build (0.6.0-A player mode)

| Item | Value |
|------|--------|
| **Output** | `Builds/Windows/CCS-Survival-0.6.0-A/CCS_Survival.exe` |
| **Summary log** | `Logs/Build_0_6_0_A.log` |
| **Unity batch log** | `Logs/Unity_Batch_0_6_0_A.log` |
| **Prefix** | `[CCS 0.6.0-A]` |
| **Mode** | Player mode — `enableTraversalTest` **off** |
| **Result** | **Pass** — build succeeded |

### Player-mode smoke (~32s, no input simulation)

| Metric | Count |
|--------|------:|
| Exception | 0 |
| LogError | 0 |
| NullReferenceException | 0 |
| MissingReferenceException | 0 |
| UnassignedReferenceException | 0 |
| Cannot set the parent | 0 |
| Core health OK | 1 |
| Survival validation rules passed | 1 |
| Interaction scanner errors (`scanOrigin` / UnassignedReference) | 0 |
| Pickup collected | 0 |

Local log copy (not committed): `Logs/Player_0_6_0_A_PlayerMode.log`.

### Traversal smoke (~62s, build-time override; scene restored)

| Item | Value |
|------|--------|
| **Build** | Temporary `enableTraversalTest` on during build only; scene restored to **off** |
| **Unity batch log** | `Logs/Unity_Batch_0_6_0_A_Traversal.log` |
| **Result** | **Pass** |

| Metric | Count |
|--------|------:|
| Exception / LogError / NRE / MissingReference / UnassignedReference / Cannot set the parent | 0 |
| Traversal PASSED (`PASSED: Route`) | **10** |
| Traversal FAILED | 0 |
| Pickup collected | 0 |
| Core health OK | 1 |
| Survival validation rules passed | 1 |

Local log copy (not committed): `Logs/Player_0_6_0_A_TraversalSmoke.log`.

### Manual checks (Editor)

| Check | Status |
|-------|--------|
| Play Mode pickup UX (walk + **E** on all three pickups) | **Pending** — not runnable from Cursor automation |
| Play Mode traversal toggle (enable/disable, restore player/camera/WASD) | **Pending** — not runnable from Cursor automation |

Use player-mode **`CCS_Survival.exe`** or Editor Play Mode for manual closure before Phase Two inventory work.

### Cleanup

- Temporary build scripts removed (`Assets/CCS/Survival/Editor/Temp/CCS_StandaloneBuild_0_6_0_A*.cs`).
- Unity batch noise restored (`ProjectSettings/*`, `Assets/Settings/*`, scene YAML reorder discarded).
- Build output and Player.log copies **not** staged.

### Final status

**Phase One Complete — 0.6.0 validated.** Automated standalone bootstrap + traversal criteria **passed**. Manual Play Mode pickup UX and traversal toggle remain **pending** in Editor.

---

## Phase 1J.2 — Dev Validation Root Cleanup Patch

### Purpose

After **0.6.0** validation, default gameplay should not show transparent hazard/vitals/traversal volumes or apply hidden zone pressure. Reusable validation systems stay in the scene but are **opt-in** via **`CCS_DevValidationRoot`**.

### Version (1J.2.0)

| Reference | Value |
|-----------|--------|
| **Project version** | **0.6.1 — Phase One Cleanup Patch** |
| **`ProjectSettings.bundleVersion`** | **0.6.1** |
| **Future standalone naming** | `Builds/Windows/CCS-Survival-0.6.1-*`, prefix `[CCS 0.6.1-*]` |

### Scene changes

| Change | Detail |
|--------|--------|
| **`CCS_DevValidationRoot`** | **`m_IsActive: 0`** by default (children preserved) |
| **`CCS_GameplayPlayAreaRoot`** | New scene root; holds **`CCS_PrototypeGround`** so player has collider when dev root is off |
| **Pickups** | **`PU_WaterCanteen`** moved from hazard cluster `(-5.5, 0.35, 0.4)` → `(2.4, 0.35, 0.8)` near spawn |
| **Traversal default** | **`enableTraversalTest`** remains **off** on committed scene |

**Dev validation (opt-in):**

1. **CCS → Survival → Validation → Enable Dev Validation Root**
2. Enable **`enableTraversalTest`** on **`CCS_TraversalTestAgent`**
3. Run route validation
4. Before commit: disable traversal test and **Disable Dev Validation Root**

Editor menus: `CCS_DevValidationRootSceneSetup_Editor` (Editor-only).

### Default gameplay expectations

| Check | Expected |
|-------|----------|
| Transparent dev zone clutter | Hidden |
| Hazard / modifier overlay lines | **None** (no active zones) |
| Immediate cold/exposure from hidden zones | None |
| Interaction overlay | **None** until near pickup |
| Pickups reachable | **PU_FoodTin**, **PU_WaterCanteen**, **PU_Kindling** near spawn |

### Play Mode validation

| Check | Status |
|-------|--------|
| Default mode — dev root inactive, clean view | **Pass** (scene/config) |
| Player/camera/WASD | **Pending** manual Editor |
| Pickup UX (walk + **E**) | **Pending** manual Editor |
| Dev mode — enable root + traversal, route PASSED | **Pending** manual Editor |

### Standalone build (0.6.1-A)

| Item | Value |
|------|--------|
| **Output** | `Builds/Windows/CCS-Survival-0.6.1-A/CCS_Survival.exe` |
| **Log** | `Logs/Build_0_6_1_A.log` |
| **Prefix** | `[CCS 0.6.1-A]` |
| **Build status** | **Pending** — Unity Editor had project open (batch lock). Re-run with Editor closed: `CCS.Survival.Editor.Temp.CCS_StandaloneBuild_0_6_1_A.ExecuteWindowsDevelopmentBuild` |

**Expected Player.log smoke (~30s, dev root off):** all error metrics **0**; **Core health OK** and **Survival validation rules passed** present; no hazard/modifier telemetry from disabled dev root.

### Final status

**Phase One cleanup patch at 0.6.1.** Default bootstrap scene is gameplay-readable; dev validation is opt-in. Proceed to Phase Two inventory when manual pickup pass is complete.

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
