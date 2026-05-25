# Survival Gameplay Architecture

**Version:** 0.1.0  
**Status:** Direction document (no gameplay systems implemented)  
**Author:** James Schilz  
**Date:** 2026-05-24

This document defines how **ccs-survival** structures gameplay on top of the CCS Core Platform. It is the game-repo counterpart to [CCS Core Platform Architecture](../../Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md).

---

## Design intent

**ccs-survival** delivers survival-focused gameplay (western MMO, extraction, co-op, and related genres) as **explicit modules** composed at bootstrap time. The Core Platform provides lifecycle, services, events, and module install — not survival rules, items, or world simulation.

```text
SCN_Game_Bootstrap (future)
  └── PF_CCS_RuntimeHost
        ├── CCS_RuntimeHost (Core — unchanged by gameplay)
        ├── Game bootstrap installer(s) (manual registration)
        └── Gameplay modules (ccs.survival.*)
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
- **Product / lore:** *Reckoning*, factions, western setting — scenes, copy, content folders only
- Never encode product title into Core or upstream framework identifiers

---

## Milestone 0.1.0 boundary

This document is **direction only**. Do not implement systems, installers, or scenes until the next gameplay milestone unless explicitly scoped.

**Next implementation milestones (suggested, not committed):**

1. Game bootstrap scene + empty install plan
2. First module skeleton (`ccs.survival.character`) with smoke-less installer
3. Inventory module wired to services/events

---

## Related documents

- [Survival Module Boundaries](Survival_Module_Boundaries.md)
- [Survival Networking Authority](Survival_Networking_Authority.md)
- [Survival Persistence Direction](Survival_Persistence_Direction.md)
- [Milestone 0.1.0](../Milestones/Milestone_0.1.0_Survival_Project_Identity_Setup.md)
