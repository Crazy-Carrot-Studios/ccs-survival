# CCS Project Documentation

**Version:** 0.1.1 — Historical Documentation Cleanup

## Overview

In-project documentation for the CCS Survival composition layer, validation standards, and architecture references.

## Architecture Reference

| Document | Topic |
|----------|-------|
| [Survival Framework Architecture Gate](Survival_Framework_Architecture_Gate.md) | Ownership boundaries, identity, profiles, scenes, runtime principles |
| [Survival Runtime Foundation](Survival_Runtime_Foundation.md) | Module base classes, installer hierarchy, service marker, constants |
| [Survival Validation Standards](Survival_Validation_Standards.md) | Module ID rules, profile identity, save-safe IDs, diagnostics |
| [Survival Authority and Avatar Architecture](Survival_Authority_And_Avatar_Architecture.md) | Authority ownership, avatar representation, binding, multiplayer compatibility |
| [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md) | Composition root, host requirements, profile slots, scene validation |
| [Framework Architecture Guide](Framework_Architecture_Guide.md) | Contributor architecture guide and anti-patterns |
| [Future Gameplay Module Guidelines](Future_Gameplay_Module_Guidelines.md) | Module structure, ownership, and integration rules |
| [Versioning Policy](CCS_Versioning_Policy.md) | Rebuild version map and tag rules |

## Bootstrap Assets

| Asset | Path |
|-------|------|
| Project bootstrap scene | `Assets/CCS/Project/Scenes/SCN_CCS_Survival_Bootstrap.unity` |
| Project bootstrap prefab | `Assets/CCS/Project/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab` |
| Core validation scene | `Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity` |

## Runtime Assembly

`Assets/CCS/Project/Runtime/CCS.Project.Runtime.asmdef` → `CCS.Core.Runtime` only.  
Namespace: `CCS.Project`.

## Runtime Foundation Index

| Topic | Path |
|-------|------|
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |
| Integration markers | `Runtime/Foundation/Diagnostics/CCS_SurvivalFrameworkFutureMarkers.cs` |
| Validation | `Runtime/Foundation/Validation/` |
| Scene bootstrap | `Runtime/Foundation/Scene/` |
| Profiles | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |
| Authority / Avatar | `Runtime/Character/Authority/`, `Avatar/`, `Identity/` |

## Module structure

See [Modules README](../../Modules/README.md) for per-module folder conventions.
