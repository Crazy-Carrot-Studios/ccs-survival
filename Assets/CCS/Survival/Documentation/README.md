# CCS Survival — In-Project Documentation

**Location:** `Assets/CCS/Survival/Documentation/`  
**Milestone:** 0.3.4 — Survival Scene Bootstrap Standards  
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

## Scene bootstrap standards (0.3.4)

| Topic | Path |
|-------|------|
| Authoring guide | [Scene_Bootstrap_Standards.md](Scene_Bootstrap_Standards.md) |
| Scene rules | `Runtime/Foundation/Scene/CCS_SurvivalSceneBootstrapRules.cs` |
| Scene validation | `Runtime/Foundation/Scene/CCS_SurvivalSceneBootstrapValidationUtility.cs` |
| Profile slot | `Runtime/Foundation/Bootstrap/CCS_SurvivalBootstrapProfileSlot.cs` |

- One `CCS_RuntimeHost` + one `CCS_SurvivalBootstrap` per scene (same composition root).
- Survival diagnostics owned by survival bootstrap; Core diagnostics off in survival scenes.
- Optional profile slots — empty during skeleton phase.
- Scene identity is not save identity.

---

## Runtime foundation

| Type | Path |
|------|------|
| Module base | `Runtime/Foundation/Modules/CCS_SurvivalModuleBase.cs` |
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |
| Validation | `Runtime/Foundation/Validation/` |
| Profile base | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |

## Authority vs Avatar (0.3.3)

| Type | Path |
|------|------|
| Authority contract | `Runtime/Character/Authority/CCS_ISurvivalAuthority.cs` |
| Avatar contract | `Runtime/Character/Avatar/CCS_ISurvivalAvatar.cs` |
| Identity validation | `Runtime/Character/Identity/CCS_SurvivalIdentityUtility.cs` |

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
| Survival README | `Assets/CCS/Survival/README.md` |

**Dependency rule:** `CCS.Survival.Runtime` → `CCS.Core.Runtime` only. Core never references Survival.

---

## Milestones (in-Unity)

| Milestone | Path |
|-----------|------|
| 0.3.4 Scene bootstrap | [Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md](Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md) |
| 0.3.3 Authority/Avatar | [Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md](Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md) |
| 0.3.2 Validation | [Milestones/Milestone_0.3.2_Survival_Module_Validation_Diagnostics_Rules.md](Milestones/Milestone_0.3.2_Survival_Module_Validation_Diagnostics_Rules.md) |

---

## Repository documentation (primary)

| Topic | Path (from repo root) |
|-------|------------------------|
| Project overview | `README.md` |
| Gameplay architecture | `Documentation/Architecture/Survival_Gameplay_Architecture.md` |

---

## Milestone 0.3.4 rule

**Scene bootstrap standards and validation only.** No gameplay mechanics, services, updatables, or Core modifications.
