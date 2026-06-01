# CCS Gameplay Modules

**Version:** 1.0.0  
**Author:** James Schilz  
**Date:** 2026-05-31

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

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.SaveLoad.Editor.CCS_SaveLoadValidationMenu.ValidateSaveLoad `
  -logFile Logs/CCS_SaveLoadValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.TimeOfDay.Editor.CCS_TimeOfDayValidationMenu.ValidateTimeOfDay `
  -logFile Logs/CCS_TimeOfDayValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.Weather.Editor.CCS_WeatherValidationMenu.ValidateWeather `
  -logFile Logs/CCS_WeatherValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.EnvironmentEffects.Editor.CCS_EnvironmentEffectsValidationMenu.ValidateEnvironmentEffects `
  -logFile Logs/CCS_EnvironmentEffectsValidation.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Modules.Shelter.Editor.CCS_ShelterValidationMenu.ValidateShelter `
  -logFile Logs/CCS_ShelterValidation.log

Unity.exe -batchmode -nographics -quit `
  -projectPath . `
  -executeMethod CCS.Modules.Building.Editor.CCS_BuildingValidationMenu.ValidateBuilding `
  -logFile Logs/CCS_BuildingValidation.log
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
| `CharacterController/` | 0.3.8, **0.9.0** — **Complete** (foundation + playable player integration) |
| `Interaction/` | 0.3.9 — **Complete** (foundation) |
| `Inventory/` | 0.4.0 — **Complete** (foundation) |
| `Equipment/` | 0.4.1 / 0.7.4 — **Foundation + environmental modifiers complete** |
| `Crafting/` | 0.5.0 / 0.5.3 — **Foundation + gameplay integration complete** |
| `WorldResources/` | 0.5.1 / 0.5.2 — **Foundation + harvest integration complete** |
| `SaveLoad/` | 0.6.0 – 0.6.2 — **Foundation + inventory/equipment persistence complete** |
| `TimeOfDay/` | 0.7.0 — **Foundation complete** |
| `Weather/` | 0.7.1 — **Foundation complete** |
| `Shelter/` | 0.7.5 — **Environmental protection foundation complete** |
| `Building/` | 0.8.0–0.8.5 — **Definition catalog, placement, persistence restore, and shelter integration complete** |
| `Gathering/` | 0.9.9 — **Resource gathering complete** (SmallTree/Rock/Bush nodes, inventory grants) |
| `Combat/` | 0.9.8 — **Primitive combat complete** (melee hunting, wildlife health, carcass on kill) |
| `Wildlife/` | 0.9.7+ — Passive wildlife AI (wander/idle/flee) + 0.9.8 health/damageable |
| `Sleep/` | 0.9.6 — **Sleep & bedroll foundation complete** (time advance, fatigue restore, shelter modifier) |
| `Cooking/` | 0.9.5 — **Consumables & hunger usage complete** (passive drain, F consume, HUD feedback) |
| `EnvironmentEffects/` | 0.7.2 / 0.7.5 — **Foundation + shelter and equipment modifiers complete** |
| `SurvivalCore/` | 0.3.7 / 0.7.5 — **Foundation + effective environment integration complete** |

See [Survival Module Roadmap](../Survival/Documentation/CCS_Survival_Module_Roadmap.md).
