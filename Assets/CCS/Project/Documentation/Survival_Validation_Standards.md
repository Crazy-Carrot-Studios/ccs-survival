# Survival Validation Standards

## Overview

Survival validation standards define how modules, profiles, and identities are checked at bootstrap time. Validation produces structured outcomes that map to `CCS_Result` at system boundaries without running on per-frame hot paths.

Consistent validation prevents invalid module registration, unstable save keys, and duplicate identity formats across gameplay features.

## Architecture

### Validation types

| Type | Path | Role |
|------|------|------|
| `CCS_SurvivalValidationResult` | `Runtime/Foundation/Validation/` | Outcome type: success, warning, or fail |
| `CCS_SurvivalModuleValidationUtility` | `Runtime/Foundation/Validation/` | Module ID, registry, service, and updatable checks |
| `CCS_SurvivalProfileBase` | `Runtime/Foundation/Profiles/` | Abstract ScriptableObject profile contract |
| `CCS_SurvivalProfileValidationUtility` | `Runtime/Foundation/Profiles/` | Profile ID and field validation |
| `CCS_SurvivalIdentityUtility` | `Runtime/Character/Identity/` | Save-stable ID format validation |

### Validation philosophy

| Principle | Rule |
|-----------|------|
| Timing | Bootstrap and diagnostics only |
| Ownership | Survival utilities validate survival rules; Core validates host and registry contracts |
| Outcomes | Use `CCS_SurvivalValidationResult`; convert to `CCS_Result` at boundaries |
| Warnings vs failures | Invalid module IDs fail bootstrap; optional unassigned profiles warn or pass |
| DRY | Identity format checks live in `CCS_SurvivalIdentityUtility` |

## Rules

### Module validation

| Rule | Requirement |
|------|-------------|
| Module ID prefix | Must start with `ccs.survival.` |
| Duplicate IDs | None permitted in module registry |
| Module count | Must match expected count for bootstrap profile |
| Services | Must match expected service count unless module phase registers services |
| Update systems | Must match expected updatable count unless module phase registers systems |
| Bootstrap installers | Exactly one survival composition installer on runner |

### Profile identity

| Concept | Rule |
|---------|------|
| Profiles | ScriptableObject setup assets — configuration only |
| Runtime state | Lives in modules and services, never in profile assets |
| `profileId` | Stable reverse-DNS ID using `ccs.survival.profile.*` prefix |
| Forbidden keys | Unity asset paths, scene references, `GetInstanceID()`, GameObject names |

### Save-safe identity

Persist stable string IDs only:

- Module IDs
- Profile IDs
- Authority IDs
- Avatar IDs
- Item and entity definition IDs

Never persist Unity asset paths, scene object references, hierarchy paths, or instance IDs as authoritative keys.

## Validation

### Enforcement points

- `CCS_SurvivalDiagnostics` during bootstrap when diagnostics are enabled
- `CCS_SurvivalModuleValidationUtility` for module registry checks
- `CCS_SurvivalProfileValidationUtility` for assigned profile slots and assets
- `CCS_SurvivalIdentityUtility` for all stable ID format checks

### Diagnostics expectations

- Log categories come from `CCS_SurvivalRuntimeConstants`
- Core diagnostics remain disabled on project bootstrap scenes
- Feature diagnostics extend survival patterns; do not invoke Core smoke tests from gameplay scenes
- Document expected module, service, and updatable counts per bootstrap profile

## Usage

Validate a module ID before registration:

```text
CCS_SurvivalIdentityUtility.ValidateModuleId(moduleId)
  → CCS_SurvivalValidationResult
  → CCS_Result at bootstrap boundary
```

Validate an assigned profile at bootstrap:

```text
CCS_SurvivalProfileValidationUtility.ValidateProfile(profile)
  → checks profileId prefix and required fields
```

## Related Documentation

- [Survival Runtime Foundation](Survival_Runtime_Foundation.md)
- [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md)
- [Survival Authority and Avatar Architecture](../../../../Documentation/Architecture/Survival_Authority_And_Avatar_Architecture.md)
- [Framework Architecture Guide](../../../../Documentation/Planning/Framework_Architecture_Guide.md)
