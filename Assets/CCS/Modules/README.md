# CCS Gameplay Modules

**Version:** 0.5.2  
**Author:** James Schilz  
**Date:** 2026-05-28

## Folder rules

| Zone | Path |
|------|------|
| **Modules** | `Assets/CCS/Modules/` |
| **Project shell** | `Assets/CCS/Survival/` |
| **Framework** | `Assets/CCS/Framework/` |

| Path | Purpose |
|------|---------|
| `Assets/CCS/Modules/` | Gameplay feature modules (`ccs.survival.*` module IDs) |
| `Assets/CCS/Survival/` | Survival project shell — bootstrap, scenes, profiles, composition, project roadmap docs |
| `Assets/CCS/Framework/` | Reusable Core Platform (upstream-aligned) |

Do **not** place gameplay modules under `Assets/CCS/Survival/Runtime/<ModuleName>/`.

Project configuration assets (for example default tuning profiles) belong under `Assets/CCS/Survival/Profiles/`, not inside module folders.

## Editor tool policy

| Category | Examples | Policy |
|----------|----------|--------|
| **Allowed (permanent)** | Validation, database authoring, content generation | Keep; register via validation pipeline where applicable |
| **Temporary** | Testing utilities, migration utilities | Runtime code may remain; remove editor menus when no longer needed |
| **Remove after use** | Setup wizards, debug convenience menus, temporary test menus | Delete once the asset or workflow is committed |

**0.3.7b:** Removed one-time Survival Core profile creation menu and development testing convenience menus. Validation menus and pipeline remain.

## Unity batch validation (before commit / tag)

Run after compile succeeds. Policy: **0 errors, 0 warnings** (warnings fail in batchmode).

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe"
$project = "<PROJECT_PATH>"

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Survival.Editor.Development.CCS_SurvivalValidationMenu.RunSurvivalValidation `
  -logFile Logs/CCS_Validation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.SurvivalCore.Editor.CCS_SurvivalCoreValidationMenu.ValidateSurvivalCore `
  -logFile Logs/CCS_SurvivalCoreValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.CharacterController.Editor.CCS_CharacterControllerValidationMenu.ValidateCharacterController `
  -logFile Logs/CCS_CharacterControllerValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.Interaction.Editor.CCS_InteractionValidationMenu.ValidateInteraction `
  -logFile Logs/CCS_InteractionValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.Inventory.Editor.CCS_InventoryValidationMenu.ValidateInventory `
  -logFile Logs/CCS_InventoryValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.Equipment.Editor.CCS_EquipmentValidationMenu.ValidateEquipment `
  -logFile Logs/CCS_EquipmentValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.UI.Editor.CCS_UIValidationMenu.ValidateUI `
  -logFile Logs/CCS_UIValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.Crafting.Editor.CCS_CraftingValidationMenu.ValidateCrafting `
  -logFile Logs/CCS_CraftingValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.WorldResources.Editor.CCS_WorldResourceValidationMenu.ValidateWorldResources `
  -logFile Logs/CCS_WorldResourcesValidation.log
```

Exit code **0** required for each run. Do not tag a milestone if validation fails.

## Standard module layout

```text
Assets/CCS/Modules/<ModuleName>/
  Runtime/           # Runtime scripts + CCS.Modules.<Name>.Runtime.asmdef
  Editor/            # Editor scripts + CCS.Modules.<Name>.Editor.asmdef (validation only when possible)
  Documentation/     # Module-specific docs (optional)
  Tests/             # Feature tests (optional)
  Prefabs/           # Feature prefabs (optional)
  Settings/          # ScriptableObject config (optional)
  UI/                # Feature UI (optional)
```

## Namespaces

- Runtime: `CCS.Modules.<ModuleName>`
- Editor: `CCS.Modules.<ModuleName>.Editor`
- Survival shell/bootstrap only: `CCS.Survival.*`

## Planned modules (roadmap)

| Folder | Milestone |
|--------|-----------|
| `SurvivalCore/` | 0.3.7 / 0.3.7a / 0.3.7b |
| `CharacterController/` | 0.3.8 — **Complete** (foundation) |
| `Interaction/` | 0.3.9 — **Complete** (foundation) |
| `Inventory/` | 0.4.0 — **Complete** (foundation) |
| `Equipment/` | 0.4.1 — **Complete** (foundation) |
| `Crafting/` | 0.5.0 — **Complete** (foundation) |
| `WorldResources/` | 0.5.1 — **Complete** (foundation) |

See [Survival Module Roadmap](../Survival/Documentation/CCS_Survival_Module_Roadmap.md).
