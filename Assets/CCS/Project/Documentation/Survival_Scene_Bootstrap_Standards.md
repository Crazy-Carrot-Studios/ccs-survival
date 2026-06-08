# Survival Scene Bootstrap Standards

## Overview

Every project bootstrap scene must use a single, predictable composition root before gameplay systems load. These standards define host requirements, bootstrap ownership, profile slot rules, and validation expectations for all survival scenes.

## Architecture

### Composition root

Place on **one** GameObject (reference: `PF_CCS_Survival_BootstrapRoot`):

| Component | Role |
|-----------|------|
| `CCS_RuntimeHost` | Core runtime host — single instance per scene |
| `CCS_SurvivalBootstrap` | Project composition root: context, installer pipeline, diagnostics |

### Startup flow

```text
CCS_RuntimeHost.Awake (Core init)
    → CCS_SurvivalBootstrap.Awake
        → CCS_SurvivalRuntimeContext.Initialize()
        → CCS_SurvivalInstaller pipeline (modules)
        → CCS_SurvivalDiagnostics (when enabled)
```

### Foundation types

| Type | Path |
|------|------|
| `CCS_SurvivalSceneBootstrapRules` | `Runtime/Foundation/Scene/` |
| `CCS_SurvivalSceneBootstrapValidationUtility` | `Runtime/Foundation/Scene/` |
| `CCS_SurvivalBootstrapProfileSlot` | `Runtime/Foundation/Bootstrap/` |
| Bootstrap scene | `Scenes/SCN_CCS_Survival_Bootstrap.unity` |

### Diagnostics ownership

| Scene type | Diagnostics owner |
|------------|-------------------|
| Project bootstrap | Project diagnostics enabled; Core diagnostics disabled |
| Core validation (`SCN_CCS_Bootstrap`) | Core smoke tests enabled |

## Rules

### Composition requirements

| Rule | Requirement |
|------|-------------|
| Runtime hosts | Exactly `ExpectedRuntimeHostCount` (= 1) |
| Bootstrap components | Exactly `ExpectedSurvivalBootstrapCount` (= 1) on same root as host |
| Runtime context | One `CCS_SurvivalRuntimeContext` per bootstrap |
| Installer registration | Manual pipeline through `CCS_SurvivalInstaller` only |
| Scene scanning | No `FindObjectOfType` or scene-wide discovery for composition |

### Registry expectations

Bootstrap profile defines expected counts for modules, services, updatables, and installers. Default skeleton profile expects one module, zero services, zero updatables, and one bootstrap installer.

### Scene identity

Scene names, hierarchy paths, and GameObject names are **not** save identity. Use stable IDs:

- Module IDs (`ccs.survival.*`)
- Authority and avatar IDs
- Profile IDs (`ccs.survival.profile.*`)
- Bootstrap slot IDs (`ccs.survival.bootstrap.slot.*`)

### Profile slots

`CCS_SurvivalBootstrap` may serialize optional `CCS_SurvivalBootstrapProfileSlot` entries:

| Field | Purpose |
|-------|---------|
| `slotId` | Stable setup slot ID (`ccs.survival.bootstrap.slot.*`) |
| `profile` | Optional `CCS_SurvivalProfileBase` asset reference |
| `isRequired` | When true, profile must be assigned and valid |

Profile slots configure setup only. They are not runtime simulation state. Empty slots do not fail bootstrap. No `Resources`, Addressables, or save IO in foundation assemblies.

## Validation

`CCS_SurvivalSceneBootstrapValidationUtility` enforces:

- Single runtime host and bootstrap on composition root
- Diagnostics ownership (project on, core off for bootstrap scenes)
- Registry counts match bootstrap profile expectations
- Assigned profile slots pass profile validation
- No duplicate bootstrap installers

Validation runs once during bootstrap — not per frame.

## Usage

Compose a new bootstrap scene:

```text
1. Add PF_CCS_Survival_BootstrapRoot (or equivalent)
2. Ensure CCS_RuntimeHost + CCS_SurvivalBootstrap on same GameObject
3. Disable Core runtime diagnostics on the host
4. Enable project diagnostics on bootstrap (default)
5. Register module installers through CCS_SurvivalInstaller only
6. Add profile slots when a module requires setup profiles
```

## Related Documentation

- [Survival Runtime Foundation](Survival_Runtime_Foundation.md)
- [Survival Validation Standards](Survival_Validation_Standards.md)
- [Survival Authority and Avatar Architecture](Survival_Authority_And_Avatar_Architecture.md)
- [Framework Architecture Guide](Framework_Architecture_Guide.md)
