# CCS Survival — Project Shell

**Milestone:** 0.3.4 — Survival Scene Bootstrap Standards  
**Author:** James Schilz  
**Date:** 2026-05-24

## 0.3.4 purpose

Define survival-owned **scene bootstrap standards** and validation helpers so every survival scene uses one predictable composition root — without gameplay mechanics.

## Scene bootstrap standards

| Rule | Detail |
|------|--------|
| Composition root | One `CCS_RuntimeHost` + one `CCS_SurvivalBootstrap` on the same GameObject |
| Context | `CCS_SurvivalRuntimeContext` bound to the host |
| Diagnostics | Survival-owned; Core diagnostics disabled in survival scenes |
| Profile slots | Optional `CCS_SurvivalBootstrapProfileSlot` list (default empty) |
| Scene identity | Not save identity — use stable module/profile/authority IDs |

**Guide:** [Documentation/Scene_Bootstrap_Standards.md](Documentation/Scene_Bootstrap_Standards.md)

## Authority vs Avatar (0.3.3)

| Layer | Contract | Role |
|-------|----------|------|
| **Authority** | `CCS_ISurvivalAuthority` | Ownership, stable `AuthorityId`, future player/network/save signals |
| **Avatar** | `CCS_ISurvivalAvatar` | Scene body (`AvatarRoot`), spawn/possession flags |
| **Identity** | `CCS_SurvivalIdentityUtility` | Save-stable ID validation |

## Foundation layer

| Type | Path |
|------|------|
| Module base | `Runtime/Foundation/Modules/CCS_SurvivalModuleBase.cs` |
| Scene rules | `Runtime/Foundation/Scene/CCS_SurvivalSceneBootstrapRules.cs` |
| Scene validation | `Runtime/Foundation/Scene/CCS_SurvivalSceneBootstrapValidationUtility.cs` |
| Profile slot | `Runtime/Foundation/Bootstrap/CCS_SurvivalBootstrapProfileSlot.cs` |
| Profiles | `Runtime/Foundation/Profiles/CCS_SurvivalProfileBase.cs` |
| Validation | `Runtime/Foundation/Validation/` |

## Character skeleton

- Module ID: `ccs.survival.character`
- Bootstrap scene: `Scenes/SCN_CCS_Survival_Bootstrap.unity`

## Skeleton diagnostics expectations

| Check | Expected |
|-------|----------|
| Modules | 1 |
| Services | 0 |
| Update systems | 0 |
| Bootstrap installers | 1 |

## What it does not own yet

- Gameplay, movement, input, controller, networking, save implementation
- Inventory, attributes, combat, AI
- Required profile slots or gameplay profile assets

## Architecture direction

```text
Framework/Core          → reusable platform (protected)
Survival/Foundation     → validation, profiles, scene bootstrap standards
Survival/Character      → authority/avatar contracts + module skeleton
Survival/<Feature>/     → future gameplay modules
```

**Dependency rule:** Survival → Core (never upward).

## Runtime assembly

`Assets/CCS/Survival/Runtime/CCS.Survival.Runtime.asmdef` references **`CCS.Core.Runtime` only**.

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Milestone 0.3.4](Documentation/Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md)
- [Scene bootstrap standards](Documentation/Scene_Bootstrap_Standards.md)
