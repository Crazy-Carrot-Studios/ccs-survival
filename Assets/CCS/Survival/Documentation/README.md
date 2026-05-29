# CCS Survival — In-Project Documentation

**Location:** `Assets/CCS/Survival/Documentation/`  
**Milestone:** 0.3.6 — Development Framework Support Foundation  
**Author:** James Schilz  
**Date:** 2026-05-28

## Development Framework Support (0.3.6)

| Guide | Path |
|-------|------|
| Development support (0.3.6) | [CCS_Survival_Development_Framework_Support.md](CCS_Survival_Development_Framework_Support.md) |
| Module roadmap | [CCS_Survival_Module_Roadmap.md](CCS_Survival_Module_Roadmap.md) |

Gameplay modules begin **after** this milestone.

## Framework Quality Gate Completed (0.3.5)

Pre-gameplay audits and contributor documentation are complete at **v0.3.5**. Gameplay modules should follow:

| Guide | Path |
|-------|------|
| Architecture (authoritative) | [Framework_Architecture_Guide.md](Framework_Architecture_Guide.md) |
| Gameplay module guidelines | [Future_Gameplay_Module_Guidelines.md](Future_Gameplay_Module_Guidelines.md) |
| Scene bootstrap | [Scene_Bootstrap_Standards.md](Scene_Bootstrap_Standards.md) |

---

## Bootstrap assets

| Asset | Path |
|-------|------|
| Survival bootstrap scene | `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity` |
| Survival bootstrap prefab | `Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab` |

**Core validation scene:** `Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity`

---

## Runtime foundation index

| Topic | Path |
|-------|------|
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |
| FUTURE markers | `Runtime/Foundation/Diagnostics/CCS_SurvivalFrameworkFutureMarkers.cs` |
| Validation | `Runtime/Foundation/Validation/` |
| Scene bootstrap | `Runtime/Foundation/Scene/` |
| Profiles | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |
| Authority / Avatar | `Runtime/Character/Authority/`, `Avatar/`, `Identity/` |
| Development support | `Runtime/Development/`, `Editor/Development/` |

---

## Milestones

| Milestone | Path |
|-----------|------|
| 0.3.6 Development support | [CCS_Survival_Development_Framework_Support.md](CCS_Survival_Development_Framework_Support.md) |
| 0.3.5 Quality gate | [Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md](Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md) |
| 0.3.4 Scene bootstrap | [Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md](Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md) |
| 0.3.3 Authority/Avatar | [Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md](Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md) |

---

## Milestone 0.3.6 rule

**Development / framework support only.** Diagnostics, validation, testing toggles, settings placeholders, and scene bootstrap dev tooling. **No gameplay mechanics.**
