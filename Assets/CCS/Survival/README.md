# CCS Survival — Project Shell

**Milestone:** 0.3.0 — Survival Character Module Skeleton  
**Author:** James Schilz  
**Date:** 2026-05-24

## 0.3.0 purpose

Establish the first **survival-owned gameplay module boundary** (`ccs.survival.character`) without implementing character gameplay mechanics.

## What the character layer owns (now)

- Module identity and metadata (`CCS_SurvivalCharacterModule`)
- Install/uninstall lifecycle via Core contracts (`CCS_SurvivalCharacterModuleInstaller`)
- Diagnostic labels and module ID constants (`CCS_SurvivalCharacterDiagnostics`)
- Registration sequencing from `CCS_SurvivalInstaller` (survival composition root)

**Location:** `Assets/CCS/Survival/Runtime/Character/`

## What it does not own yet

- Player controller, movement, camera rigs
- Attributes, hunger, thirst, stamina
- Inventory, equipment, crafting, hotbar
- Combat, AI, animation gameplay
- Save/load character state
- Multiplayer replication or network packages
- Avatar/presentation separation (planned later)

## Architecture direction

```text
Framework/Core          → reusable platform (protected)
Framework/Modules       → shared genre modules (future)
Survival/Runtime        → survival bootstrap + character layer (this repo)
```

**Dependency rule:** Survival → Modules → Core (never upward).

Player **authority** and **avatar** separation will be introduced in a later milestone after the character module boundary is stable.

## Runtime assembly

`Assets/CCS/Survival/Runtime/CCS.Survival.Runtime.asmdef` references **`CCS.Core.Runtime` only**.

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Milestone 0.2.0](../../Documentation/Milestones/Milestone_0.2.0_Survival_Bootstrap_Scene_Empty_Install_Pipeline.md)
- [Survival gameplay architecture](../../Documentation/Architecture/Survival_Gameplay_Architecture.md)
