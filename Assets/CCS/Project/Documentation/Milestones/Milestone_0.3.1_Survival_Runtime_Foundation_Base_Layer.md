# Milestone 0.3.1 — Survival Runtime Foundation Base Layer

**Version:** 0.3.1  
**Status:** Foundation milestone (no gameplay)  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Predecessor:** [Milestone 0.3.0a](../../Documentation/Milestones/Milestone_0.3.0a_Survival_Character_Stability_Hotfix.md) (`v0.3.0a`)

**Goal:** Survival-owned base wrappers and markers for AAA-consistent module/service conventions before gameplay contracts begin.

---

## Purpose

Future survival modules (Attributes, Inventory bridge, Character Authority, Needs, Combat, Save) should inherit shared Survival foundation types instead of scattering direct Core usage across feature folders.

This milestone adds **structure only** — no mechanics.

---

## Files added

| File | Role |
|------|------|
| `Runtime/Foundation/Modules/CCS_SurvivalModuleBase.cs` | Abstract survival module base (`CCS_ModuleBase`) |
| `Runtime/Foundation/Modules/CCS_SurvivalModuleInstallerBase.cs` | Abstract survival installer base (`CCS_ModuleInstallerBase`) |
| `Runtime/Foundation/Services/CCS_ISurvivalService.cs` | Marker for future survival services (`CCS_IService`) |
| `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` | Central module IDs, log categories, diagnostic expectations |

---

## Refactored (behavior unchanged)

| File | Change |
|------|--------|
| `CCS_SurvivalCharacterModule` | Inherits `CCS_SurvivalModuleBase` |
| `CCS_SurvivalCharacterModuleInstaller` | Inherits `CCS_SurvivalModuleInstallerBase` |
| `CCS_SurvivalCharacterDiagnostics` | Aliases foundation constants |
| `CCS_SurvivalDiagnostics` | Uses `ExpectedSkeletonModuleCount` constant |
| `CCS_SurvivalInstaller` | Uses foundation log category constant |

---

## Why this is not gameplay

- No inventory, attributes, hunger/thirst, combat, AI, save, or networking
- No services registered on `CCS_ServiceRegistry`
- No updatables registered on `CCS_RuntimeUpdateLoop`
- No player controller or simulation rules
- Lifecycle hooks remain skeleton logs only

---

## How future modules should use the base layer

```text
CCS_SurvivalYourFeatureModule : CCS_SurvivalModuleBase
CCS_SurvivalYourFeatureModuleInstaller : CCS_SurvivalModuleInstallerBase
CCS_IYourFeatureService : CCS_ISurvivalService   (when needed)
Module ID: CCS_SurvivalRuntimeConstants.ModuleIdPrefix + "yourfeature"
```

Register from `CCS_SurvivalInstaller` in explicit order — no auto-discovery.

---

## Core vs Survival ownership

| Layer | Owns |
|-------|------|
| **Core** | `CCS_ModuleBase`, `CCS_ModuleInstallerBase`, `CCS_IService`, host/registry |
| **Survival Foundation** | Wrappers, constants, survival service marker |
| **Survival Features** | Feature modules under `Runtime/<Feature>/` |

**Rule:** Survival → Core (never upward). Core must not reference Survival.

---

## Validation expectations (unchanged)

- `Modules=1` (`ExpectedSkeletonModuleCount`)
- `Services=0`
- `BootstrapInstallers=1` (survival composition installer on runner)

---

## Related documents

- [Survival README](../README.md)
- [Survival gameplay architecture](../../../Documentation/Architecture/Survival_Gameplay_Architecture.md)
