# CCS Survival — Project Shell

**Milestone:** 0.3.8a — Batch validation runner hotfix  
**Author:** James Schilz  
**Date:** 2026-05-28

## Folder rules

| Path | Purpose |
|------|---------|
| `Assets/CCS/Modules/` | Gameplay modules (Survival Core, Character Controller, …) |
| `Assets/CCS/Survival/` | Bootstrap, scenes, profiles, composition, project roadmap docs |
| `Assets/CCS/Framework/` | Reusable Core Platform |

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

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Module roadmap](Documentation/CCS_Survival_Module_Roadmap.md)
- [Character Controller module](../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md)
- [Survival Core module](../Modules/SurvivalCore/Documentation/CCS_Survival_Core_Module.md)
- [Gameplay modules index](../Modules/README.md)
