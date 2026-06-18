# CCS Project Documentation

**Version:** 0.2.1 — Character Controller Test Ground

## Overview

Project-specific documentation for the CCS Survival composition layer: bootstrap rules, runtime foundation, validation standards, and architecture gate.

Broader planning and contributor guides live in the repo [`Documentation/`](../../../../Documentation/README.md) folder.

## Project documentation

| Document | Topic |
|----------|-------|
| [Survival Framework Architecture Gate](Survival_Framework_Architecture_Gate.md) | Ownership boundaries, identity, profiles, scenes, runtime principles |
| [Survival Runtime Foundation](Survival_Runtime_Foundation.md) | Module base classes, installer hierarchy, service marker, constants |
| [Survival Validation Standards](Survival_Validation_Standards.md) | Module ID rules, profile identity, save-safe IDs, diagnostics |
| [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md) | Composition root, host requirements, profile slots, scene validation |
| [Versioning Policy](CCS_Versioning_Policy.md) | Rebuild version map and tag rules |

## Bootstrap assets

| Asset | Path |
|-------|------|
| Project bootstrap scene | `Assets/CCS/Project/Scenes/SCN_CCS_Survival_Bootstrap.unity` |
| Project bootstrap prefab | `Assets/CCS/Project/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab` |
| Core validation scene | `Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity` |

## Runtime assembly

`Assets/CCS/Project/Runtime/CCS.Project.Runtime.asmdef` → `CCS.Core.Runtime` only.  
Namespace: `CCS.Project`.

## Runtime foundation index

| Topic | Path |
|-------|------|
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |
| Integration markers | `Runtime/Foundation/Diagnostics/CCS_SurvivalFrameworkFutureMarkers.cs` |
| Validation | `Runtime/Foundation/Validation/` |
| Scene bootstrap | `Runtime/Foundation/Scene/` |
| Profiles | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |
| Authority / Avatar | `Runtime/Character/Authority/`, `Avatar/`, `Identity/` |

## Modules

Module conventions and active module docs live under [`Assets/CCS/Modules/`](../Modules/README.md).

Character Controller module doc: [`CCS_CharacterController_Module.md`](../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md)
