# CCS Survival — In-Project Documentation

**Location:** `Assets/CCS/Survival/Documentation/`  
**Milestone:** 0.3.3 — Survival Authority Avatar Boundary Skeleton  
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

## Runtime foundation

| Type | Path |
|------|------|
| Module base | `Runtime/Foundation/Modules/CCS_SurvivalModuleBase.cs` |
| Installer base | `Runtime/Foundation/Modules/CCS_SurvivalModuleInstallerBase.cs` |
| Service marker | `Runtime/Foundation/Services/CCS_ISurvivalService.cs` |
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |
| Validation | `Runtime/Foundation/Validation/` |
| Profile base | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |

## Authority vs Avatar (0.3.3)

| Type | Path |
|------|------|
| Authority contract | `Runtime/Character/Authority/CCS_ISurvivalAuthority.cs` |
| Avatar contract | `Runtime/Character/Avatar/CCS_ISurvivalAvatar.cs` |
| Binding | `Runtime/Character/Avatar/CCS_SurvivalAuthorityAvatarBinding.cs` |
| Identity validation | `Runtime/Character/Identity/CCS_SurvivalIdentityUtility.cs` |
| Authority/avatar validation | `Runtime/Character/Avatar/CCS_SurvivalAuthorityAvatarValidationUtility.cs` |

- **Authority** — ownership, save/network identity signals (no netcode dependency today).
- **Avatar** — scene representation only (`Transform` root); not persistent ownership identity.
- Save keys use stable `AuthorityId` / `profileId`, not scene objects or asset paths.

## Profile-driven setup direction

- Future modules may use ScriptableObject profiles for setup/configuration.
- Profiles are assets; runtime state and save data remain separate.
- `profileId` must be save-stable (`ccs.survival.profile.*`) — not asset paths.

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
| 0.3.3 Authority/Avatar | [Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md](Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md) |
| 0.3.2 Validation | [Milestones/Milestone_0.3.2_Survival_Module_Validation_Diagnostics_Rules.md](Milestones/Milestone_0.3.2_Survival_Module_Validation_Diagnostics_Rules.md) |
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

## Milestone 0.3.3 rule

**Authority/avatar contracts and identity validation only.** No gameplay mechanics, services, updatables, or Core modifications.
