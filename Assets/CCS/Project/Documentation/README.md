# CCS Project — In-Project Documentation

**Location:** `Assets/CCS/Project/Documentation/`  
**Version:** 0.0.3 — Controlled Rebuild Baseline  
**Author:** James Schilz  
**Date:** 2026-06-07

## Folder ownership (`Assets/CCS/`)

| Folder | Owns |
|--------|------|
| `Framework/` | CCS Core platform only (upstream-aligned) |
| `Modules/` | All gameplay/system modules and module-owned data |
| `Shared/` | Cross-module assets used by 2+ modules |
| `Project/` | Bootstrap, composition, scenes, project docs, install sequencing |
| `Tests/` | Cross-cutting edit/play mode test results and harnesses |

**No global `Database/` folder.** Module-owned data lives inside the module that owns it.

## Framework quality gate (v0.3.5)

Pre-gameplay audits and contributor documentation are complete. Gameplay modules should follow:

| Guide | Path |
|-------|------|
| Versioning policy | [CCS_Versioning_Policy.md](CCS_Versioning_Policy.md) |
| Architecture (authoritative) | [Framework_Architecture_Guide.md](Framework_Architecture_Guide.md) |
| Gameplay module guidelines | [Future_Gameplay_Module_Guidelines.md](Future_Gameplay_Module_Guidelines.md) |
| Scene bootstrap | [Scene_Bootstrap_Standards.md](Scene_Bootstrap_Standards.md) |

## Bootstrap assets

| Asset | Path |
|-------|------|
| Project bootstrap scene | `Assets/CCS/Project/Scenes/SCN_CCS_Survival_Bootstrap.unity` |
| Project bootstrap prefab | `Assets/CCS/Project/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab` |

**Core validation scene:** `Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity`

## Runtime assembly

`Assets/CCS/Project/Runtime/CCS.Project.Runtime.asmdef` → references `CCS.Core.Runtime` only.  
Namespace: `CCS.Project`. Class names retain `CCS_Survival*` prefix until a dedicated rename pass.

## Runtime foundation index

| Topic | Path |
|-------|------|
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |
| FUTURE markers | `Runtime/Foundation/Diagnostics/CCS_SurvivalFrameworkFutureMarkers.cs` |
| Validation | `Runtime/Foundation/Validation/` |
| Scene bootstrap | `Runtime/Foundation/Scene/` |
| Profiles | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |
| Authority / Avatar | `Runtime/Character/Authority/`, `Avatar/`, `Identity/` |

## Milestones

| Milestone | Path |
|-----------|------|
| 0.3.5 Quality gate | [Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md](Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md) |
| 0.3.4 Scene bootstrap | [Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md](Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md) |
| 0.3.3 Authority/Avatar | [Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md](Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md) |

## Milestone 0.3.5 rule

**Documentation and framework hardening only.** No gameplay mechanics, services, updatables, or Core modifications.
