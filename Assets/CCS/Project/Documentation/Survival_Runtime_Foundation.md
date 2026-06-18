# Survival Runtime Foundation

## Overview

The survival runtime foundation provides shared base types, service markers, and diagnostic constants so gameplay modules inherit consistent conventions instead of scattering direct Core usage across feature folders.

Foundation types define structure and contracts only. They do not implement gameplay mechanics, simulation rules, or persistence.

## Architecture

### Ownership boundaries

| Layer | Owns |
|-------|------|
| **Core** | `CCS_ModuleBase`, `CCS_ModuleInstallerBase`, `CCS_IService`, host, registry, update loop |
| **Project foundation** | Survival wrappers, constants, service marker, validation utilities |
| **Modules** | Feature modules under `Assets/CCS/Modules/<Feature>/` |

Dependency direction is strict: Modules → Project → Core. Core must never reference Project or Modules.

### Foundation types

| Type | Path | Role |
|------|------|------|
| `CCS_SurvivalModuleBase` | `Runtime/Foundation/Modules/` | Abstract survival module base extending `CCS_ModuleBase` |
| `CCS_SurvivalModuleInstallerBase` | `Runtime/Foundation/Modules/` | Abstract installer base extending `CCS_ModuleInstallerBase` |
| `CCS_ISurvivalService` | `Runtime/Foundation/Services/` | Marker interface for survival services extending `CCS_IService` |
| `CCS_SurvivalRuntimeConstants` | `Runtime/Foundation/Diagnostics/` | Module ID prefixes, log categories, diagnostic expectations |

### Module hierarchy

```text
CCS_SurvivalFeatureModule : CCS_SurvivalModuleBase
CCS_SurvivalFeatureModuleInstaller : CCS_SurvivalModuleInstallerBase
CCS_IFeatureService : CCS_ISurvivalService          (when a service is required)
Module ID: CCS_SurvivalRuntimeConstants.ModuleIdPrefix + "feature"
```

Installers register through `CCS_SurvivalInstaller` in explicit order. Auto-discovery is not permitted.

## Rules

- All survival gameplay modules inherit `CCS_SurvivalModuleBase` and `CCS_SurvivalModuleInstallerBase`.
- Module IDs use the `ccs.survival.` prefix defined in `CCS_SurvivalRuntimeConstants`.
- Log categories must come from `CCS_SurvivalRuntimeConstants` — do not hardcode category strings.
- Foundation assemblies reference `CCS.Core.Runtime` only.
- Foundation must not register gameplay services or updatables unless a module phase explicitly requires them.
- Gameplay logic belongs in `Assets/CCS/Modules/`, not in Project foundation types.

## Validation

Bootstrap diagnostics enforce foundation expectations:

| Check | Requirement |
|-------|-------------|
| Module count | Matches `ExpectedSkeletonModuleCount` for current bootstrap profile |
| Services | Matches `SkeletonExpectedServicesCount` unless a module registers services |
| Update systems | Matches `SkeletonExpectedUpdateSystemsCount` unless a module registers updatables |
| Bootstrap installers | Exactly one survival composition installer on the bootstrap runner |
| Module IDs | Valid prefix, no duplicates in registry |

Validation runs at bootstrap and diagnostics only — never per frame.

## Usage

Register a new module installer from `CCS_SurvivalInstaller`:

```text
1. Create module class inheriting CCS_SurvivalModuleBase
2. Create installer inheriting CCS_SurvivalModuleInstallerBase
3. Assign stable module ID with ccs.survival.* prefix
4. Register installer in explicit install order
```

Feature services implement `CCS_ISurvivalService` and register on `CCS_ServiceRegistry` during module install when required.

## Related Documentation

- [Survival Validation Standards](Survival_Validation_Standards.md)
- [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md)
- [Survival Authority and Avatar Architecture](../../../../Documentation/Architecture/Survival_Authority_And_Avatar_Architecture.md)
- [Framework Architecture Guide](../../../../Documentation/Planning/Framework_Architecture_Guide.md)
