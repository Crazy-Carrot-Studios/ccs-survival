# CCS Survival — Project Shell

**Milestone:** 0.5.1 — World Resource Module Foundation  
**Author:** James Schilz  
**Date:** 2026-05-28

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

## World Resources (0.5.1)

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

## Equipment (0.4.1 / 0.4.1a)

| Area | Path |
|------|------|
| Runtime | `../Modules/Equipment/Runtime/` |
| Editor validation | `../Modules/Equipment/Editor/Validation/` |
| Default profile | `Profiles/Equipment/CCS_DefaultEquipmentProfile.asset` |
| Module doc | [../Modules/Equipment/Documentation/CCS_Equipment_Module.md](../Modules/Equipment/Documentation/CCS_Equipment_Module.md) |

**0.4.1a:** Carry capacity modifiers (`Back`, `Satchel`, `Bedroll` slots) expose additional inventory slots and carry weight through `CCS_PlayerEquipmentService`.

**Editor menu:** **CCS → Survival → Equipment → Validate Equipment**

## Inventory (0.4.0)

| Area | Path |
|------|------|
| Runtime | `../Modules/Inventory/Runtime/` |
| Editor validation | `../Modules/Inventory/Editor/Validation/` |
| Default profile | `Profiles/Inventory/CCS_DefaultInventoryProfile.asset` |
| Module doc | [../Modules/Inventory/Documentation/CCS_Inventory_Module.md](../Modules/Inventory/Documentation/CCS_Inventory_Module.md) |

**Editor menu:** **CCS → Survival → Inventory → Validate Inventory**

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
