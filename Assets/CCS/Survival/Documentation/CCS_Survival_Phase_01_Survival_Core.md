# CCS Survival — Phase 1 Survival Core

**Document Type:** Phase Engineering Plan  
**Project:** CCS Survival  
**Phase:** 1 — Survival Core  
**Author:** James Schilz  
**Date:** 2026-05-27  
**Status:** Phase 1E — Player Movement + New Input System (Implemented)

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
- Dynamic glyph UI and **stamina** sprint gating still deferred
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
