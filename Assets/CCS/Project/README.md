# CCS Project — Composition Shell

**Location:** `Assets/CCS/Project/`  
**Milestone:** 0.3.5 — Framework Quality Gate + folder normalization  
**Author:** James Schilz  
**Date:** 2026-06-07

## What Project owns

- Game bootstrap and install sequencing (`CCS_SurvivalBootstrap`, `CCS_SurvivalInstaller`)
- Project runtime context, validation, and diagnostics contracts
- Bootstrap scenes and composition prefabs
- Project-specific documentation and milestone records
- Character module **skeleton** (transitional — first full gameplay rebuild moves to `Assets/CCS/Modules/`)

## What Project does not own

- Gameplay module implementations (→ `Assets/CCS/Modules/`)
- Cross-module shared assets (→ `Assets/CCS/Shared/`)
- Core platform code (→ `Assets/CCS/Framework/`)
- Module-owned data assets (→ inside each module)

## Framework quality gate (v0.3.5)

- Architecture boundaries verified (Project → Core only)
- Identity philosophy documented (authority, avatar, profile, scene, runtime)
- Validation deduplicated and bootstrap-time only
- Diagnostics ownership confirmed (project-owned)
- Contributor guides and anti-patterns published

**Authoritative guides:**

- [Framework Architecture Guide](Documentation/Framework_Architecture_Guide.md)
- [Future Gameplay Module Guidelines](Documentation/Future_Gameplay_Module_Guidelines.md)
- [Scene Bootstrap Standards](Documentation/Survival_Scene_Bootstrap_Standards.md)

## Runtime assembly

`Assets/CCS/Project/Runtime/CCS.Project.Runtime.asmdef` references **`CCS.Core.Runtime` only**.

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Shared folder purpose](../Shared/README.md)
- [Architecture Gate](Documentation/Survival_Framework_Architecture_Gate.md)
