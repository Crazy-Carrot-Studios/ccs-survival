# Survival Gameplay Architecture

**Version:** 0.2.0  
**Status:** Direction document + bootstrap shell (no gameplay modules implemented)  
**Author:** James Schilz  
**Date:** 2026-05-24

This document defines how **ccs-survival** structures gameplay on top of the CCS Core Platform. It is the game-repo counterpart to [CCS Core Platform Architecture](../../Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md).

---

## Design intent

**ccs-survival** delivers survival-focused gameplay (extraction, co-op, multiplayer, and related modes) as **explicit modules** composed at bootstrap time. The Core Platform provides lifecycle, services, events, and module install — not survival rules, items, or world simulation.

```text
SCN_CCS_Survival_Bootstrap (implemented 0.2.0)
  └── PF_CCS_Survival_BootstrapRoot
        ├── CCS_RuntimeHost (Core — unchanged by gameplay; Core diagnostics OFF)
        ├── CCS_SurvivalBootstrap (survival-owned startup pipeline)
        └── Gameplay modules (ccs.survival.*) — future, via survival install sequencing
              ├── Character / movement
              ├── Inventory & equipment
              ├── Crafting & recipes
              ├── UI / HUD
              └── Save (game contracts; Core save foundations when upstream exists)
```

---

## Layering model

| Layer | Responsibility | Location |
|-------|----------------|----------|
| **Core Platform** | Host, bootstrap runner, module registry, services, events, diagnostics | `Assets/CCS/Framework/Core/` |
| **Gameplay modules** | Feature systems with `CCS_IModule` + installers | `Assets/CCS/Modules/<Feature>/` |
| **Survival shell** | Cross-cutting game identity, shared constants, install plan docs | `Assets/CCS/Survival/` |
| **Content** | Scenes, prefabs, ScriptableObjects, art/audio | Under modules or `Assets/` game trees (TBD per feature) |

Gameplay never patches Core types for convenience. Extend via modules, services registered on the host, and events.

---

## Bootstrap composition (planned)

1. Load game bootstrap scene (index 0 or game-defined — separate from `SCN_CCS_Bootstrap` Core validation).
2. Ensure `PF_CCS_RuntimeHost` exists (or equivalent prefab instance).
3. Register **game install plan** on `CCS_RuntimeHost.BootstrapRunner` in explicit order.
4. Each module installer extends `CCS_ModuleInstallerBase`, installs module, registers with `ModuleHost`.
5. Module systems register updatables on `RuntimeUpdateLoop` during install.

**No auto-discovery:** install order is a checked-in plan (code or ScriptableObject list), not scene scanning.

---

## Module granularity (target)

| Module ID (example) | Scope |
|---------------------|--------|
| `ccs.survival.character` | Movement, stance, camera hooks |
| `ccs.survival.inventory` | Containers, items, pickup |
| `ccs.survival.equipment` | Wearables, stat hooks |
| `ccs.survival.crafting` | Recipes, stations |
| `ccs.survival.hotbar` | Quick-use bar |
| `ccs.survival.ui` | Game HUD and menus |
| `ccs.survival.save` | Game save orchestration (uses persistence direction doc) |

Dependencies are declared via `CCS_IModuleDependencyProvider` for **preflight validation only**; install order remains manual.

---

## Communication patterns

| Pattern | Use when |
|---------|----------|
| **Services** (`CCS_ServiceRegistry`) | Stable cross-module APIs (e.g. inventory query interface) |
| **Events** (`CCS_EventDispatcher`) | One-to-many notifications (item picked up, craft completed) |
| **Direct module reference** | Avoid; prefer service or event after install |

All handlers must tolerate missing modules during bootstrap ordering tests.

---

## Simulation ownership

- **Server-authoritative** simulation is the long-term default for multiplayer survival (see [Survival Networking Authority](Survival_Networking_Authority.md)).
- **Client prediction** may apply to local movement and UI feedback; server reconciles inventory and world state.
- Single-player uses the same ownership rules locally (one logical authority) to avoid split-brain code paths later.

---

## Product vs platform naming

- **Platform / repo:** `ccs-survival`, module IDs `ccs.survival.*`
- **Project content:** game-specific scenes, copy, profiles, and content folders — not framework identifiers
- Never encode a single product title into Core or upstream framework identifiers

---

## Milestone 0.2.0 boundary

Bootstrap scene and empty installer shell exist. **No gameplay modules** are registered yet.

**Next implementation milestones (suggested, not committed):**

1. First module skeleton (`ccs.survival.character`) registered from `CCS_SurvivalInstaller`
2. Documented manual module install order in survival installer
3. Inventory module wired to services/events (later milestone)

---

## Related documents

- [Survival Module Boundaries](Survival_Module_Boundaries.md)
- [Survival Networking Authority](Survival_Networking_Authority.md)
- [Survival Persistence Direction](Survival_Persistence_Direction.md)
- [Milestone 0.1.0](../Milestones/Milestone_0.1.0_Survival_Project_Identity_Setup.md)
