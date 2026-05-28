# CCS Survival — Phase 1 Survival Core

**Document Type:** Phase Engineering Plan  
**Project:** CCS Survival  
**Phase:** 1 — Survival Core  
**Author:** James Schilz  
**Date:** 2026-05-27  
**Status:** Planning (No Implementation)

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
| `CCS_ISurvivalService` | Service contract for reading/updating survival state |
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
