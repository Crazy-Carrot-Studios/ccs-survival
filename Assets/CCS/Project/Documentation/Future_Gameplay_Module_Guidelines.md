# Future Gameplay Module Guidelines

## Overview

Guidelines for adding gameplay modules under `Assets/CCS/Modules/` after architecture normalization.

## 1. Folder ownership

| Location | Owns |
|----------|------|
| `Assets/CCS/Framework/` | Core platform only |
| `Assets/CCS/Modules/<Feature>/` | Gameplay modules and **module-owned data** |
| `Assets/CCS/Project/` | Bootstrap, composition, project docs |

Create `Assets/CCS/Shared/` only when an asset is actively used by 2+ modules.  
Create `Assets/CCS/Tests/` only when a cross-cutting test harness is actively needed.

**Do not scaffold unused module folders.** Only **CharacterController** exists today.

---

## 2. When to create a gameplay module

Create a new module when a feature:

- Has distinct install/teardown lifecycle needs
- Registers services or updatables intentionally
- Owns a stable `ccs.survival.<feature>` module ID
- Should validate independently at bootstrap

Do **not** create a module for one-off helpers or single MonoBehaviour utilities.

---

## 3. Recommended module structure

```text
Assets/CCS/Modules/<Feature>/
  Runtime/             → CCS.Modules.<Feature>.Runtime.asmdef
  Editor/              → validation menus, authoring tools
  Content/             → module-owned data (items, recipes, definitions)
  Prefabs/             → module test prefabs (required before next module)
  Profiles/            → module ScriptableObject profiles
  Tests/               → module-scoped tests
  Documentation/       → module contract and integration notes
```

Register installers in `CCS_SurvivalInstaller` (Project layer) with **explicit order**.

Project foundation types (`CCS_SurvivalModuleBase`, `CCS_SurvivalProfileBase`, identity utilities) remain in `CCS.Project.Runtime`.

---

## 4. When to use profiles

| Use profiles for | Do not use profiles for |
|------------------|-------------------------|
| Tunable setup data (rates, caps, tables) | Live simulation state |
| Editor/content configuration | Per-frame mutable values |
| Schema-versioned content packs | Authority/session identity |
| Feature enable flags at setup time | Network replication state |

Assign profiles through serialized references or bootstrap slots — not `Resources` or Addressables in foundation assemblies.

---

## 5. Authority vs avatar in gameplay modules

- **Input and decisions** → authority layer (`CCS_ISurvivalAuthority` implementations).
- **Body, visuals, sockets, camera targets** → avatar layer (`CCS_ISurvivalAvatar`).
- Gameplay modules must not collapse authority ID and avatar ID into one string.

Possession flow (future): bind avatar to authority using `CCS_SurvivalAuthorityAvatarBinding` validation at setup time.

---

## 6. Save identity guidance

Persist:

- `AuthorityId`, `profileId`, module-scoped entity IDs, item definition IDs

Never persist as primary keys:

- Scene names, hierarchy paths, GameObject names
- Unity asset paths (`Assets/...`)
- `GetInstanceID()` values
- Bootstrap slot IDs (setup only)

---

## 7. Diagnostics expectations

- Add a feature log category constant (or alias in feature `Diagnostics` class).
- Extend project diagnostics with feature checks behind explicit calls — do not spam per-frame logs.
- Keep Core diagnostics **off** in project bootstrap scenes.
- Document expected module/service/updatable counts per milestone.

---

## 8. Validation expectations

- Add static validation utilities for feature-specific IDs and config.
- Reuse `CCS_SurvivalIdentityUtility` for string ID format checks.
- Return `CCS_SurvivalValidationResult`; convert to `CCS_Result` at bootstrap boundaries.
- Run validation during install/bootstrap — not in `Update`.

---

## 9. Modularity expectations

- Manual installer registration only.
- Declare module dependencies in metadata for preflight validation.
- No static global gameplay state; use context/host-owned registries.
- Feature assemblies may reference `CCS.Project.Runtime` + `CCS.Core.Runtime`; Project foundation must not reference feature assemblies.

---

## 10. Performance expectations

- No per-frame scene queries (`FindObjectsByType`) for composition.
- No allocations in validation hot paths (bootstrap runs once).
- Prefer readonly structs for small validation outcomes (existing pattern).
- Systems that tick register as Core updatables deliberately with documented ownership.

---

## 11. MMO / scaling considerations (future)

- **Authority-first replication:** simulation ownership follows authority, not mesh instance IDs.
- **Streaming:** each loaded partition should respect one composition root per project context.
- **Content IDs:** use stable definition IDs in data; resolve assets from content maps at load time.
- **Module expansion:** add modules incrementally with install order documented in milestone notes.

See `CCS_SurvivalFrameworkFutureMarkers` for planned integration strings.

---

## 12. Checklist before merging a gameplay module

- [ ] Module ID uses `ccs.survival.` prefix
- [ ] Code lives under `Assets/CCS/Modules/<Feature>/`
- [ ] Module-owned data lives inside the module (not global `Database/`)
- [ ] Working test prefab or test scene asset included
- [ ] Validation menu or bootstrap validation included
- [ ] Module documentation included
- [ ] Installer registered explicitly in Project installer
- [ ] No Core modifications unless upstream-worthy
- [ ] Profiles contain setup data only
- [ ] Save keys documented (stable IDs only)
- [ ] Diagnostics category defined
- [ ] Milestone doc updated

Do not create the next module folder until the current module passes validation.

---

## Related

- [Framework Architecture Guide](Framework_Architecture_Guide.md)
- [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md)
- [Modules README](../../Modules/README.md)
