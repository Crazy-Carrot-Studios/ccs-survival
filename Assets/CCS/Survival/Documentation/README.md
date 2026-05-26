# CCS Survival — In-Project Documentation

**Location:** `Assets/CCS/Survival/Documentation/`  
**Milestone:** 0.3.1 — Survival Runtime Foundation Base Layer  
**Author:** James Schilz  
**Date:** 2026-05-24

Unity-visible index for survival-specific documentation and bootstrap assets.

---

## Bootstrap assets (0.2.0)

| Asset | Path |
|-------|------|
| Survival bootstrap scene | `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity` |
| Survival bootstrap prefab | `Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab` |

**Core validation scene (do not modify for survival features):** `Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity`

---

## Runtime foundation (0.3.1)

| Type | Path |
|------|------|
| Module base | `Runtime/Foundation/Modules/CCS_SurvivalModuleBase.cs` |
| Installer base | `Runtime/Foundation/Modules/CCS_SurvivalModuleInstallerBase.cs` |
| Service marker | `Runtime/Foundation/Services/CCS_ISurvivalService.cs` |
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |

---

## Runtime shell

| Type | Path |
|------|------|
| Assembly | `Assets/CCS/Survival/Runtime/CCS.Survival.Runtime.asmdef` |
| Bootstrap | `Runtime/Bootstrap/CCS_SurvivalBootstrap.cs` |
| Installer | `Runtime/Installers/CCS_SurvivalInstaller.cs` |
| Diagnostics | `Runtime/Diagnostics/CCS_SurvivalDiagnostics.cs` |
| Context | `Runtime/Context/CCS_SurvivalRuntimeContext.cs` |
| Character module | `Runtime/Character/Modules/CCS_SurvivalCharacterModule.cs` |
| Character installer | `Runtime/Character/Modules/CCS_SurvivalCharacterModuleInstaller.cs` |
| Character diagnostics | `Runtime/Character/Diagnostics/CCS_SurvivalCharacterDiagnostics.cs` |
| Survival README | `Assets/CCS/Survival/README.md` |

**Dependency rule:** `CCS.Survival.Runtime` → `CCS.Core.Runtime` only. Core never references Survival.

---

## Milestones (in-Unity)

| Milestone | Path |
|-----------|------|
| 0.3.1 Foundation | [Milestones/Milestone_0.3.1_Survival_Runtime_Foundation_Base_Layer.md](Milestones/Milestone_0.3.1_Survival_Runtime_Foundation_Base_Layer.md) |

---

## Repository documentation (primary)

| Topic | Path (from repo root) |
|-------|------------------------|
| Project overview | `README.md` |
| Gameplay architecture | `Documentation/Architecture/Survival_Gameplay_Architecture.md` |
| Module boundaries | `Documentation/Architecture/Survival_Module_Boundaries.md` |

---

## Core Platform (protected — read before editing)

| Topic | Path |
|-------|------|
| Core architecture | `Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md` |
| Script standards | `Assets/CCS/Framework/Documentation/CCS_Script_Standards.md` |

Do not add survival gameplay logic under `Assets/CCS/Framework/Core/`.

---

## Milestone 0.3.1 rule

**Foundation wrappers only.** No gameplay mechanics, services, updatables, or Core modifications.
