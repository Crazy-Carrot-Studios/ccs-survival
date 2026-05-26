# CCS Survival — In-Project Documentation

**Location:** `Assets/CCS/Survival/Documentation/`  
**Milestone:** 0.3.0 — Survival Character Module Skeleton  
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

## Runtime shell (0.2.0)

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

**Dependency rule:** `CCS.Survival.Runtime` → `CCS.Core.Runtime` only. Character skeleton types use `namespace CCS.Survival` in the same assembly. Core never references Survival.

---

## Repository documentation (primary)

| Topic | Path (from repo root) |
|-------|------------------------|
| Project overview | `README.md` |
| Gameplay architecture | `Documentation/Architecture/Survival_Gameplay_Architecture.md` |
| Module boundaries | `Documentation/Architecture/Survival_Module_Boundaries.md` |
| Networking authority | `Documentation/Architecture/Survival_Networking_Authority.md` |
| Persistence direction | `Documentation/Architecture/Survival_Persistence_Direction.md` |
| Milestone 0.2.0 | `Documentation/Milestones/Milestone_0.2.0_Survival_Bootstrap_Scene_Empty_Install_Pipeline.md` |

---

## Core Platform (protected — read before editing)

| Topic | Path |
|-------|------|
| Core architecture | `Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md` |
| Upstream workflow | `Assets/CCS/Framework/Core/Documentation/CCS_Upstream_Workflow.md` |
| Script standards | `Assets/CCS/Framework/Documentation/CCS_Script_Standards.md` |

Do not add survival gameplay logic under `Assets/CCS/Framework/Core/`.

---

## Sibling folders

| Folder | Purpose |
|--------|---------|
| `Assets/CCS/Survival/Scenes/` | Game entry bootstrap scenes |
| `Assets/CCS/Survival/Prefabs/` | Survival bootstrap prefabs |
| `Assets/CCS/Survival/Runtime/` | `CCS.Survival.Runtime` shell + character skeleton |
| `Assets/CCS/Modules/` | Gameplay feature modules (`ccs.survival.*`) — not wired at 0.2.0 |
| `Assets/CCS/Framework/` | Vendored CCS Core Platform |

---

## Milestone 0.3.0 rule

**Character module skeleton only.** No movement, attributes, inventory, combat, save, or networking packages.
