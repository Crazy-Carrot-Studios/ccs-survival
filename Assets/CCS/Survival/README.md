# CCS Survival — Project Shell

**Milestone:** 0.3.2 — Survival Module Validation + Diagnostics Rules  
**Author:** James Schilz  
**Date:** 2026-05-24

## 0.3.2 purpose

Add survival-owned **validation rules** and a **profile foundation** so future modules are easier to verify, harder to misuse, and prepared for save-stable identity — without gameplay mechanics.

## Foundation layer

| Type | Path |
|------|------|
| Module base | `Runtime/Foundation/Modules/CCS_SurvivalModuleBase.cs` |
| Installer base | `Runtime/Foundation/Modules/CCS_SurvivalModuleInstallerBase.cs` |
| Service marker | `Runtime/Foundation/Services/CCS_ISurvivalService.cs` |
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |
| Validation | `Runtime/Foundation/Validation/` |
| Profiles | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |

## Profile-driven setup direction

Future survival systems may use **ScriptableObject profiles** for editor/setup configuration:

- Profiles are **configuration assets**, not runtime simulation state.
- Runtime state and future save data stay **separate** from profile assets.
- `profileId` uses stable reverse-DNS IDs (`ccs.survival.profile.*`) — never Unity asset paths or scene references.
- `CCS_SurvivalProfileValidationUtility` enforces save-friendly ID rules at foundation layer.

## Character skeleton

- Module ID: `ccs.survival.character`
- Validated on install via `CCS_SurvivalModuleValidationUtility`
- **Location:** `Assets/CCS/Survival/Runtime/Character/`

## Skeleton diagnostics expectations

| Check | Expected |
|-------|----------|
| Modules | 1 |
| Services | 0 |
| Update systems | 0 |
| Bootstrap installers | 1 |

## What it does not own yet

- Player controller, attributes, hunger/thirst, inventory, combat, AI, save implementation, networking
- Gameplay profile assets with tuning data
- Registered survival services or updatables

## Architecture direction

```text
Framework/Core          → reusable platform (protected)
Survival/Foundation     → wrappers, validation, profiles, constants
Survival/Character      → first feature skeleton
Survival/<Feature>/     → future gameplay modules
```

**Dependency rule:** Survival → Core (never upward).

## Runtime assembly

`Assets/CCS/Survival/Runtime/CCS.Survival.Runtime.asmdef` references **`CCS.Core.Runtime` only**.

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Milestone 0.3.2](Documentation/Milestones/Milestone_0.3.2_Survival_Module_Validation_Diagnostics_Rules.md)
- [Survival gameplay architecture](../../Documentation/Architecture/Survival_Gameplay_Architecture.md)
