# Future Gameplay Module Guidelines

**Milestone:** 0.3.5  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Audience:** Contributors adding gameplay modules after foundation quality gate

---

## 1. When to create a gameplay module

Create a new survival module when a feature:

- Has distinct install/teardown lifecycle needs
- Registers services or updatables intentionally (post-skeleton milestones)
- Owns a stable `ccs.survival.<feature>` module ID
- Should validate independently at bootstrap

Do **not** create a module for one-off helpers or single MonoBehaviour utilities.

---

## 2. Recommended structure

```text
Assets/CCS/Survival/Runtime/<Feature>/
  Modules/           → CCS_Survival<Module> + Installer
  Services/          → CCS_ISurvivalService implementations (when needed)
  Diagnostics/       → feature log category aliases
  (optional) Profiles/ → feature profile types inheriting CCS_SurvivalProfileBase
```

Register installers in `CCS_SurvivalInstaller` (or a successor) with **explicit order**.

---

## 3. When to use profiles

| Use profiles for | Do not use profiles for |
|------------------|-------------------------|
| Tunable setup data (rates, caps, tables) | Live simulation state |
| Editor/content configuration | Per-frame mutable values |
| Schema-versioned content packs | Authority/session identity |
| Feature enable flags at setup time | Network replication state |

Assign profiles through serialized references or bootstrap slots — not `Resources` or Addressables in foundation assemblies.

---

## 4. Authority vs avatar in gameplay modules

- **Input and decisions** → authority layer (`CCS_ISurvivalAuthority` implementations).
- **Body, visuals, sockets, camera targets** → avatar layer (`CCS_ISurvivalAvatar`).
- Gameplay modules must not collapse authority ID and avatar ID into one string.

Possession flow (future): bind avatar to authority using `CCS_SurvivalAuthorityAvatarBinding` validation at setup time.

---

## 5. Save identity guidance

Persist:

- `AuthorityId`, `profileId`, module-scoped entity IDs, item definition IDs

Never persist as primary keys:

- Scene names, hierarchy paths, GameObject names
- Unity asset paths (`Assets/...`)
- `GetInstanceID()` values
- Bootstrap slot IDs (setup only)

---

## 6. Diagnostics expectations

- Add a feature log category constant (or alias in feature `Diagnostics` class).
- Extend survival diagnostics with feature checks behind explicit calls — do not spam per-frame logs.
- Keep Core diagnostics **off** in survival gameplay scenes.
- Document expected module/service/updatable counts per milestone.

---

## 7. Validation expectations

- Add static validation utilities for feature-specific IDs and config.
- Reuse `CCS_SurvivalIdentityUtility` for string ID format checks.
- Return `CCS_SurvivalValidationResult`; convert to `CCS_Result` at bootstrap boundaries.
- Run validation during install/bootstrap — not in `Update`.

---

## 8. Modularity expectations

- Manual installer registration only.
- Declare module dependencies in metadata for preflight validation.
- No static global gameplay state; use context/host-owned registries.
- Feature assemblies may reference `CCS.Survival.Runtime` + `CCS.Core.Runtime`; Survival foundation must not reference feature assemblies.

---

## 9. Performance expectations

- No per-frame scene queries (`FindObjectsByType`) for composition.
- No allocations in validation hot paths (bootstrap runs once).
- Prefer readonly structs for small validation outcomes (existing pattern).
- Systems that tick register as Core updatables deliberately with documented ownership.

---

## 10. MMO / scaling considerations (future)

- **Authority-first replication:** simulation ownership follows authority, not mesh instance IDs.
- **Streaming:** each loaded partition should respect one composition root per survival context.
- **Content IDs:** use stable definition IDs in data; resolve assets from content maps at load time.
- **Module expansion:** add modules incrementally with install order documented in milestone notes.

See `CCS_SurvivalFrameworkFutureMarkers` for planned integration strings.

---

## 11. Checklist before merging a gameplay module

- [ ] Module ID uses `ccs.survival.` prefix
- [ ] Installer registered explicitly
- [ ] No Core modifications unless upstream-worthy
- [ ] Profiles contain setup data only
- [ ] Save keys documented (stable IDs only)
- [ ] Diagnostics category defined
- [ ] Validation runs at bootstrap
- [ ] Milestone doc updated

---

## Related

- [Framework Architecture Guide](Framework_Architecture_Guide.md)
- [Scene Bootstrap Standards](Scene_Bootstrap_Standards.md)
