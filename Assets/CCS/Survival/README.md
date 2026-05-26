# CCS Survival — Project Shell

**Milestone:** 0.3.1 — Survival Runtime Foundation Base Layer  
**Author:** James Schilz  
**Date:** 2026-05-24

## 0.3.1 purpose

Add survival-owned **foundation wrappers and markers** so future gameplay modules share consistent module, installer, service, and diagnostic conventions — without adding gameplay mechanics.

## Foundation layer (new)

| Type | Path |
|------|------|
| Module base | `Runtime/Foundation/Modules/CCS_SurvivalModuleBase.cs` |
| Installer base | `Runtime/Foundation/Modules/CCS_SurvivalModuleInstallerBase.cs` |
| Service marker | `Runtime/Foundation/Services/CCS_ISurvivalService.cs` |
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |

## Character skeleton (refactored onto foundation)

- `CCS_SurvivalCharacterModule` → inherits `CCS_SurvivalModuleBase`
- `CCS_SurvivalCharacterModuleInstaller` → inherits `CCS_SurvivalModuleInstallerBase`
- Module ID unchanged: `ccs.survival.character`

**Location:** `Assets/CCS/Survival/Runtime/Character/`

## What it does not own yet

- Player controller, movement, camera rigs
- Attributes, hunger, thirst, stamina
- Inventory, equipment, crafting, hotbar
- Combat, AI, animation gameplay
- Save/load character state
- Multiplayer replication or network packages
- Registered survival services or updatables

## Architecture direction

```text
Framework/Core          → reusable platform (protected)
Survival/Foundation     → survival module/service wrappers + constants
Survival/Character      → first feature skeleton (0.3.0)
Survival/<Feature>/     → future gameplay modules
```

**Dependency rule:** Survival → Core (never upward).

## Runtime assembly

`Assets/CCS/Survival/Runtime/CCS.Survival.Runtime.asmdef` references **`CCS.Core.Runtime` only**.

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Milestone 0.3.1](Documentation/Milestones/Milestone_0.3.1_Survival_Runtime_Foundation_Base_Layer.md)
- [Survival gameplay architecture](../../Documentation/Architecture/Survival_Gameplay_Architecture.md)
