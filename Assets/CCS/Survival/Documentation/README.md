# CCS Survival — In-Project Documentation

**Location:** `Assets/CCS/Survival/Documentation/`  
**Milestone:** 0.4.0 — Survival Core Prototype Start  
**Author:** James Schilz  
**Date:** 2026-05-27

## Current Version

Active prototype work at **v0.4.0** — Phase 1 survival core implementation and manual validation.

## Framework Quality Gate Completed (0.3.5)

Pre-gameplay audits and contributor documentation are complete at **v0.3.5**. Gameplay modules should follow:

| Guide | Path |
|-------|------|
| Gameplay constitution | [../../Documentation/Gameplay/CCS_Survival_Gameplay_Constitution.md](../../Documentation/Gameplay/CCS_Survival_Gameplay_Constitution.md) |
| Gameplay systems breakdown | [../../Documentation/Gameplay/CCS_Survival_Gameplay_Systems_Breakdown.md](../../Documentation/Gameplay/CCS_Survival_Gameplay_Systems_Breakdown.md) |
| Gameplay loop specification | [../../Documentation/Gameplay/CCS_Survival_Gameplay_Loop_Specification.md](../../Documentation/Gameplay/CCS_Survival_Gameplay_Loop_Specification.md) |
| Reputation & law spec | [../../Documentation/Gameplay/CCS_Survival_Reputation_And_Law_Design_Spec.md](../../Documentation/Gameplay/CCS_Survival_Reputation_And_Law_Design_Spec.md) |
| Architecture (authoritative) | [Framework_Architecture_Guide.md](Framework_Architecture_Guide.md) |
| Gameplay module guidelines | [Future_Gameplay_Module_Guidelines.md](Future_Gameplay_Module_Guidelines.md) |
| Scene bootstrap | [Scene_Bootstrap_Standards.md](Scene_Bootstrap_Standards.md) |
| Prototype roadmap | [CCS_Survival_Prototype_Roadmap.md](CCS_Survival_Prototype_Roadmap.md) |
| Phase 1 — Survival Core plan | [CCS_Survival_Phase_01_Survival_Core.md](CCS_Survival_Phase_01_Survival_Core.md) |

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

---

## Milestones

| Milestone | Path |
|-----------|------|
| 0.3.5 Quality gate | [Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md](Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md) |
| 0.3.4 Scene bootstrap | [Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md](Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md) |
| 0.3.3 Authority/Avatar | [Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md](Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md) |

---

## Milestone 0.3.5 rule

**Documentation and framework hardening only.** No gameplay mechanics, services, updatables, or Core modifications.
