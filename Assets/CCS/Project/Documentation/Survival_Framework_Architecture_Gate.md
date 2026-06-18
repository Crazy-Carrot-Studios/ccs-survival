# Survival Framework Architecture Gate

## Overview

The architecture gate defines the permanent principles that govern the survival project foundation. All gameplay modules, bootstrap scenes, and shared assets must conform to these boundaries before integration.

This document is the audit reference for ownership, identity, profiles, scenes, and runtime composition.

## Architectural Principles

- Modules → Project → Core dependency direction only.
- Manual installer registration — no auto-discovery or scene scanning.
- Instance-owned subsystems via `CCS_RuntimeHost` per runtime context.
- No singleton managers or static global service locators.
- Validation at bootstrap time — not per-frame hot paths.
- Every module must be testable in isolation with a working test prefab or scene object.
- Core platform code remains upstream-aligned and gameplay-free.

## Ownership Boundaries

| Zone | Path | Permitted | Forbidden |
|------|------|-----------|-----------|
| Core | `Assets/CCS/Framework/Core/` | Host, registry, services, events, smoke tests | Gameplay logic, survival assumptions |
| Project | `Assets/CCS/Project/` | Bootstrap, composition, validation contracts, project docs | Feature gameplay implementations |
| Modules | `Assets/CCS/Modules/<Feature>/` | Feature runtime, editor, prefabs, data, tests | Core modifications, global state |
| Shared | `Assets/CCS/Shared/` (when created) | Cross-module assets (2+ consumers) | Module-specific logic or data |
| Tests | `Assets/CCS/Tests/` (when created) | Cross-cutting harnesses | Feature-specific tests (belong in module) |

Assembly boundary: `CCS.Project.Runtime` references `CCS.Core.Runtime` only. Module assemblies may reference Project and Core.

## Identity Philosophy

All stable IDs share one format rule set:

| Identity | Prefix | Save-authoritative |
|----------|--------|--------------------|
| Module | `ccs.survival.` | Registration |
| Profile | `ccs.survival.profile.` | Setup and schema keys |
| Authority | `ccs.survival.authority.` | Yes |
| Avatar | `ccs.survival.avatar.` | Instance only |
| Binding | `ccs.survival.binding.` | Correlation only |
| Bootstrap slot | `ccs.survival.bootstrap.slot.` | Setup wiring only |

Forbidden everywhere: asset paths, scene paths, GameObject names, `GetInstanceID()`.

Centralized validation: `CCS_SurvivalIdentityUtility`.

## Profile Philosophy

| Principle | Rule |
|-----------|------|
| Purpose | Setup and configuration only |
| Base type | `CCS_SurvivalProfileBase` ScriptableObjects |
| Runtime state | Modules, services, and context — never profile assets |
| `profileId` | Save-stable string ID, not asset path |
| Bootstrap slots | Optional setup wiring on `CCS_SurvivalBootstrap` |
| Inheritance | Feature profiles derive from `CCS_SurvivalProfileBase` in module assemblies |

Anti-pattern: storing hunger, inventory, transform, or simulation state in profile assets.

## Scene Philosophy

- One `CCS_RuntimeHost` and one `CCS_SurvivalBootstrap` per bootstrap scene.
- Project scenes own project diagnostics; Core diagnostics off on gameplay bootstrap.
- Scene names and hierarchy are not save identity.
- Profile slots are optional and setup-only.
- Scene validation via `CCS_SurvivalSceneBootstrapValidationUtility`.

See [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md).

## Runtime Philosophy

| Concern | Owner |
|---------|-------|
| Module lifecycle | `CCS_SurvivalModuleBase` / installer hierarchy |
| Services | `CCS_ISurvivalService` implementations in modules |
| Simulation state | Modules and runtime context |
| Diagnostics | Project-owned; feature extensions behind explicit calls |
| Constants | `CCS_SurvivalRuntimeConstants` — single source for prefixes and log categories |
| Integration markers | `CCS_SurvivalFrameworkFutureMarkers` for planned extension points |

Netcode references are forbidden in `CCS.Project.Runtime`. Networking adapts to authority in gameplay assemblies.

## Related Documentation

- [Survival Runtime Foundation](Survival_Runtime_Foundation.md)
- [Survival Validation Standards](Survival_Validation_Standards.md)
- [Survival Authority and Avatar Architecture](Survival_Authority_And_Avatar_Architecture.md)
- [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md)
- [Framework Architecture Guide](Framework_Architecture_Guide.md)
- [Future Gameplay Module Guidelines](Future_Gameplay_Module_Guidelines.md)
