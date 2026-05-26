# CCS Survival — Project Shell

**Milestone:** 0.3.3 — Survival Authority Avatar Boundary Skeleton  
**Author:** James Schilz  
**Date:** 2026-05-24

## 0.3.3 purpose

Establish the survival-owned **authority vs avatar** boundary so future systems keep ownership identity, save keys, input intent, and scene representation separate — without movement, input, networking, or save implementation.

## Authority vs Avatar

| Layer | Contract | Role |
|-------|----------|------|
| **Authority** | `CCS_ISurvivalAuthority` | Ownership, stable `AuthorityId`, future player/network/save signals |
| **Avatar** | `CCS_ISurvivalAvatar` | Scene body (`AvatarRoot`), spawn/possession flags — not persistent ownership |
| **Binding** | `CCS_SurvivalAuthorityAvatarBinding` | Links `AuthorityId` ↔ `AvatarId` (no spawn/save IO) |
| **Identity** | `CCS_SurvivalIdentityUtility` | Save-stable ID validation (no Unity paths or instance IDs) |

**Location:** `Assets/CCS/Survival/Runtime/Character/Authority/`, `Avatar/`, `Identity/`

## Foundation layer

| Type | Path |
|------|------|
| Module base | `Runtime/Foundation/Modules/CCS_SurvivalModuleBase.cs` |
| Installer base | `Runtime/Foundation/Modules/CCS_SurvivalModuleInstallerBase.cs` |
| Service marker | `Runtime/Foundation/Services/CCS_ISurvivalService.cs` |
| Constants | `Runtime/Foundation/Diagnostics/CCS_SurvivalRuntimeConstants.cs` |
| Validation | `Runtime/Foundation/Validation/` |
| Profiles | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |

## Profile-driven setup direction

Future survival systems may use **ScriptableObject profiles** for editor/setup configuration:

- Profiles are **configuration assets**, not runtime simulation state.
- Runtime state and future save data stay **separate** from profile assets.
- `profileId` uses stable reverse-DNS IDs (`ccs.survival.profile.*`) — never Unity asset paths or scene references.

## Character skeleton

- Module ID: `ccs.survival.character`
- Validated on install via `CCS_SurvivalModuleValidationUtility`
- Bootstrap does **not** require runtime authority or avatar instances yet

## Skeleton diagnostics expectations

| Check | Expected |
|-------|----------|
| Modules | 1 |
| Services | 0 |
| Update systems | 0 |
| Bootstrap installers | 1 |

## What it does not own yet

- Movement, input, player controller, networking package, save implementation
- Inventory, attributes, combat, AI, animator, equipment
- Runtime authority/avatar implementations
- Registered survival services or updatables

## Architecture direction

```text
Framework/Core          → reusable platform (protected)
Survival/Foundation     → wrappers, validation, profiles, constants
Survival/Character      → authority/avatar contracts + module skeleton
Survival/<Feature>/     → future gameplay modules
```

**Dependency rule:** Survival → Core (never upward).

## Runtime assembly

`Assets/CCS/Survival/Runtime/CCS.Survival.Runtime.asmdef` references **`CCS.Core.Runtime` only**.

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Milestone 0.3.3](Documentation/Milestones/Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md)
- [Survival gameplay architecture](../../Documentation/Architecture/Survival_Gameplay_Architecture.md)
