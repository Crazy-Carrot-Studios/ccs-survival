# CCS Survival — Project Shell

**Milestone:** 0.8.1 — Building Placement Foundation  
**Author:** James Schilz  
**Date:** 2026-05-31

## Folder rules

| Path | Purpose |
|------|---------|
| `Assets/CCS/Modules/` | Gameplay modules (Survival Core, Character Controller, …) |
| `Assets/CCS/Survival/` | Bootstrap, scenes, profiles, composition, project roadmap docs |
| `Assets/CCS/Framework/` | Reusable Core Platform |

## Build verification (0.4.1b)

| Doc | Path |
|-----|------|
| Build verification | [Documentation/CCS_Survival_Build_Verification.md](Documentation/CCS_Survival_Build_Verification.md) |

## World Resources (0.5.1 / 0.5.2)

| Area | Path |
|------|------|
| Runtime | `../Modules/WorldResources/Runtime/` |
| Editor validation | `../Modules/WorldResources/Editor/Validation/` |
| Default profile | `Profiles/WorldResources/CCS_DefaultWorldResourceProfile.asset` |
| Module doc | [../Modules/WorldResources/Documentation/CCS_World_Resources_Module.md](../Modules/WorldResources/Documentation/CCS_World_Resources_Module.md) |

**Editor menu:** **CCS → Survival → World Resources → Validate World Resources**

## Crafting (0.5.0)

| Area | Path |
|------|------|
| Runtime | `../Modules/Crafting/Runtime/` |
| Editor validation | `../Modules/Crafting/Editor/Validation/` |
| Default profile | `Profiles/Crafting/CCS_DefaultCraftingProfile.asset` |
| Module doc | [../Modules/Crafting/Documentation/CCS_Crafting_Module.md](../Modules/Crafting/Documentation/CCS_Crafting_Module.md) |

**Editor menu:** **CCS → Survival → Crafting → Validate Crafting**

## Save / Load (0.6.0 / 0.6.1 / 0.6.2)

| Area | Path |
|------|------|
| Runtime | `../Modules/SaveLoad/Runtime/` |
| Editor validation | `../Modules/SaveLoad/Editor/Validation/` |
| Default profile | `Profiles/SaveLoad/CCS_DefaultSaveLoadProfile.asset` |
| Module doc | [../Modules/SaveLoad/Documentation/CCS_Save_Load_Module.md](../Modules/SaveLoad/Documentation/CCS_Save_Load_Module.md) |

**Editor menu:** **CCS → Survival → Save Load → Validate Save Load**

**0.6.2:** Inventory and equipment services register as saveables. Bootstrap persistence harness verifies harvest → craft → equip → save → load round-trip.

## Equipment (0.4.1 / 0.4.1a)

| Area | Path |
|------|------|
| Runtime | `../Modules/Equipment/Runtime/` |
| Editor validation | `../Modules/Equipment/Editor/Validation/` |
| Default profile | `Profiles/Equipment/CCS_DefaultEquipmentProfile.asset` |
| Module doc | [../Modules/Equipment/Documentation/CCS_Equipment_Module.md](../Modules/Equipment/Documentation/CCS_Equipment_Module.md) |

**0.4.1a:** Carry capacity modifiers (`Back`, `Satchel`, `Bedroll` slots) expose additional inventory slots and carry weight through `CCS_PlayerEquipmentService`.

**Editor menu:** **CCS → Survival → Equipment → Validate Equipment**

**0.6.2:** `CCS_PlayerEquipmentService` persists equipped slots via `CCS_EquipmentSaveData` (`saveDataVersion`). Restores after inventory on load.

## Time Of Day (0.7.0)

| Area | Path |
|------|------|
| Runtime | `../Modules/TimeOfDay/Runtime/` |
| Editor validation | `../Modules/TimeOfDay/Editor/Validation/` |
| Default profile | `Profiles/TimeOfDay/CCS_DefaultTimeOfDayProfile.asset` |
| Module doc | [../Modules/TimeOfDay/Documentation/CCS_Time_Of_Day_Module.md](../Modules/TimeOfDay/Documentation/CCS_Time_Of_Day_Module.md) |

**Editor menu:** **CCS → Survival → Time Of Day → Validate Time Of Day**

**0.7.0:** Global game clock with phases, events, save/load persistence, and bootstrap HUD time display.

## Weather (0.7.1)

| Area | Path |
|------|------|
| Runtime | `../Modules/Weather/Runtime/` |
| Editor validation | `../Modules/Weather/Editor/Validation/` |
| Default profile | `Profiles/Weather/CCS_DefaultWeatherProfile.asset` |
| Module doc | [../Modules/Weather/Documentation/CCS_Weather_Module.md](../Modules/Weather/Documentation/CCS_Weather_Module.md) |

**Editor menu:** **CCS → Survival → Weather → Validate Weather**

**0.7.1:** Global weather state with transitions, one-way Time Of Day integration, save/load persistence, and bootstrap HUD weather display. No VFX, lighting, or audio.

## Environment Effects (0.7.2)

| Area | Path |
|------|------|
| Runtime | `../Modules/EnvironmentEffects/Runtime/` |
| Editor validation | `../Modules/EnvironmentEffects/Editor/Validation/` |
| Default profile | `Profiles/EnvironmentEffects/CCS_DefaultEnvironmentEffectsProfile.asset` |
| Module doc | [../Modules/EnvironmentEffects/Documentation/CCS_Environment_Effects_Module.md](../Modules/EnvironmentEffects/Documentation/CCS_Environment_Effects_Module.md) |

**Editor menu:** **CCS → Survival → Environment Effects → Validate Environment Effects**

**0.7.2:** Ambient temperature, wetness, and exposure simulation from Time Of Day and Weather snapshots. Save/load persistence and bootstrap HUD environment display. No Survival Core stat mutation, clothing insulation, or damage.

## Survival Core Environment Integration (0.7.3)

| Area | Path |
|------|------|
| Runtime | `../Modules/SurvivalCore/Runtime/` |
| Environment influence | `../Modules/SurvivalCore/Runtime/Environment/` |
| Editor validation | `../Modules/SurvivalCore/Editor/Validation/` |
| Default profile | `Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset` |
| Module doc | [../Modules/SurvivalCore/Documentation/CCS_Survival_Core_Module.md](../Modules/SurvivalCore/Documentation/CCS_Survival_Core_Module.md) |

**Editor menu:** **CCS → Survival → Survival Core → Validate Survival Core**

**0.7.3:** Environment Effects drives temperature, fatigue, and thirst pressure through `CCS_SurvivalCoreService`. Bootstrap influence HUD panel for debug verification. No Health damage or death systems.

## Equipment Environmental Modifiers (0.7.4)

| Area | Path |
|------|------|
| Runtime bridge | `../Modules/Equipment/Runtime/Services/CCS_EquipmentEnvironmentRuntimeBridge.cs` |
| Modifier snapshot | `../Modules/Equipment/Runtime/Data/CCS_EquipmentEnvironmentalModifierSnapshot.cs` |
| Test assets | `Profiles/Equipment/TestItems/CCS_TestEquipment_WarmHat.asset`, `CCS_TestEquipment_HeavyCoat.asset`, `CCS_TestEquipment_WaterproofBoots.asset` |

**0.7.4:** Equipped items aggregate temperature, wetness, and exposure resistance. Environment Effects exposes raw and effective values on snapshots and bootstrap HUD. Survival Core consumes effective values.

## Shelter Environmental Protection (0.7.5)

| Area | Path |
|------|------|
| Runtime | `../Modules/Shelter/Runtime/` |
| Editor validation | `../Modules/Shelter/Editor/Validation/` |
| Default profile | `Profiles/Shelter/CCS_DefaultShelterProfile.asset` |
| Module doc | [../Modules/Shelter/Documentation/CCS_Shelter_Module.md](../Modules/Shelter/Documentation/CCS_Shelter_Module.md) |

**Editor menu:** **CCS → Survival → Shelter → Validate Shelter**

**0.7.5:** Trigger-volume shelter protection applied before equipment resistances. Bootstrap includes `CCS_TestShelterVolume` and `CCS_ShelterTestHarness` for development verification.

## Building Foundation (0.8.0)

| Area | Path |
|------|------|
| Runtime | `../Modules/Building/Runtime/` |
| Editor validation | `../Modules/Building/Editor/Validation/` |
| Default profile | `Profiles/Building/CCS_DefaultBuildingProfile.asset` |
| Test definitions | `Content/Building/Definitions/CCS_TestFoundation.asset`, `CCS_TestWall.asset`, `CCS_TestRoof.asset` |
| Module doc | [../Modules/Building/Documentation/CCS_Building_Module.md](../Modules/Building/Documentation/CCS_Building_Module.md) |

**Editor menu:** **CCS → Survival → Building → Validate Building**

**0.8.0:** Structure definition catalog and service persistence only. Placement, snapping, holograms, build mode, durability, repair, and demolition deferred.

## Building Placement (0.8.1)

| Area | Path |
|------|------|
| Placement service | `../Modules/Building/Runtime/Services/CCS_BuildingPlacementService.cs` |
| Preview | `../Modules/Building/Runtime/Placement/CCS_BuildingPlacementPreview.cs` |
| Test harness | `../Modules/Building/Runtime/Testing/CCS_BuildingPlacementTestHarness.cs` |
| Test area | Bootstrap scene `CCS_BuildingTestArea` |

**Editor menu:** **CCS → Survival → Building → Validate Building**

**0.8.1:** Build mode, placement preview, spawned cube instances, and placed instance save model. Bootstrap harness cycles foundation/wall/roof near the shelter test area. No inventory consumption or snapping yet.

## Inventory (0.4.0)

| Area | Path |
|------|------|
| Runtime | `../Modules/Inventory/Runtime/` |
| Editor validation | `../Modules/Inventory/Editor/Validation/` |
| Default profile | `Profiles/Inventory/CCS_DefaultInventoryProfile.asset` |
| Module doc | [../Modules/Inventory/Documentation/CCS_Inventory_Module.md](../Modules/Inventory/Documentation/CCS_Inventory_Module.md) |

**Editor menu:** **CCS → Survival → Inventory → Validate Inventory**

**0.6.2:** `CCS_PlayerInventoryService` persists slot stacks via `CCS_InventorySaveData` (`saveDataVersion`).

## Interaction (0.3.9)

| Area | Path |
|------|------|
| Runtime | `../Modules/Interaction/Runtime/` |
| Editor validation | `../Modules/Interaction/Editor/Validation/` |
| Default profile | `Profiles/Interaction/CCS_DefaultInteractionProfile.asset` |
| Module doc | [../Modules/Interaction/Documentation/CCS_Interaction_Module.md](../Modules/Interaction/Documentation/CCS_Interaction_Module.md) |

**Editor menu:** **CCS → Survival → Interaction → Validate Interaction**

## Character Controller (0.3.8)

| Area | Path |
|------|------|
| Runtime | `../Modules/CharacterController/Runtime/` |
| Editor validation | `../Modules/CharacterController/Editor/Validation/` |
| Default profile | `Profiles/CharacterController/CCS_DefaultCharacterControllerProfile.asset` |
| Module doc | [../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md](../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md) |

**Editor menu:** **CCS → Survival → Character Controller → Validate Character Controller**

## Survival Core (0.3.7 / 0.3.7a / 0.3.7b)

Stat foundation for Health, Stamina, Hunger, Thirst, Temperature, and Fatigue:

| Area | Path |
|------|------|
| Runtime | `../Modules/SurvivalCore/Runtime/` |
| Editor tools | `../Modules/SurvivalCore/Editor/` |
| Default profile | `Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset` |
| Module doc | [../Modules/SurvivalCore/Documentation/CCS_Survival_Core_Module.md](../Modules/SurvivalCore/Documentation/CCS_Survival_Core_Module.md) |

**Editor menus (permanent):**

- **CCS → Survival → Validate Survival** (foundation)
- **CCS → Survival → Survival Core → Validate Survival Core**
- **CCS → Survival → Bootstrap → Validate Scene Bootstrap**

**Default profile:** `Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset` (committed project configuration)

## Development Framework Support (0.3.6)

| Area | Path |
|------|------|
| Runtime development | `Runtime/Development/` |
| Editor development | `Editor/Development/` |
| Module roadmap | [Documentation/CCS_Survival_Module_Roadmap.md](Documentation/CCS_Survival_Module_Roadmap.md) |

## Framework Quality Gate (0.3.5)

- [Framework Architecture Guide](Documentation/Framework_Architecture_Guide.md)
- [Future Gameplay Module Guidelines](Documentation/Future_Gameplay_Module_Guidelines.md)
- [Scene Bootstrap Standards](Documentation/Scene_Bootstrap_Standards.md)

## Scene bootstrap standards (0.3.4)

| Rule | Detail |
|------|--------|
| Composition root | One `CCS_RuntimeHost` + one `CCS_SurvivalBootstrap` |
| Context | `CCS_SurvivalRuntimeContext` |

## Skeleton diagnostics expectations

| Check | Expected |
|-------|----------|
| Modules | 1 (character skeleton) |
| Services | 0 |
| Bootstrap installers | 1 |

## Runtime assemblies

| Assembly | References |
|----------|------------|
| `CCS.Survival.Runtime` | `CCS.Core.Runtime` |
| `CCS.Survival.Editor` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |
| `CCS.Modules.SurvivalCore.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |
| `CCS.Modules.SurvivalCore.Editor` | `CCS.Core.Runtime`, `CCS.Survival.Runtime`, `CCS.Survival.Editor`, `CCS.Modules.SurvivalCore.Runtime` |
| `CCS.Modules.CharacterController.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |
| `CCS.Modules.CharacterController.Editor` | Core, Survival runtime/editor, CharacterController runtime |
| `CCS.Modules.Interaction.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |
| `CCS.Modules.Interaction.Editor` | Core, Survival runtime/editor, Interaction runtime |
| `CCS.Modules.Inventory.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |
| `CCS.Modules.Inventory.Editor` | Core, Survival runtime/editor, Inventory runtime |
| `CCS.Modules.Equipment.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime`, `CCS.Modules.Inventory.Runtime` |
| `CCS.Modules.Equipment.Editor` | Core, Survival runtime/editor, Equipment runtime |

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Module roadmap](Documentation/CCS_Survival_Module_Roadmap.md)
- [Equipment module](../Modules/Equipment/Documentation/CCS_Equipment_Module.md)
- [Inventory module](../Modules/Inventory/Documentation/CCS_Inventory_Module.md)
- [Interaction module](../Modules/Interaction/Documentation/CCS_Interaction_Module.md)
- [Character Controller module](../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md)
- [Survival Core module](../Modules/SurvivalCore/Documentation/CCS_Survival_Core_Module.md)
- [Gameplay modules index](../Modules/README.md)
