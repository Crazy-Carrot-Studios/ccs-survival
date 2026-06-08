# Project Framework Architecture Guide

**Milestone:** 0.3.5 — Framework Quality Gate + folder normalization  
**Author:** James Schilz  
**Date:** 2026-06-07  
**Status:** Authoritative for `ccs-survival` foundation layer (pre-gameplay)

---

## 1. Overview

The project framework is a **composition and validation layer** above CCS Core. It prepares modules, identity contracts, profiles, scene bootstrap standards, and diagnostics — without gameplay mechanics.

```text
Assets/CCS/Framework/          → Core platform only
Assets/CCS/Project/            → bootstrap, composition, project docs
Assets/CCS/Modules/<Feature>/  → gameplay modules + module-owned data
Assets/CCS/Shared/             → cross-module assets (2+ consumers)
Assets/CCS/Tests/              → cross-cutting test harnesses
```

**Assembly rule:** `CCS.Project.Runtime` references **`CCS.Core.Runtime` only**.

---

## 2. Core vs Project vs Modules boundary

| Responsibility | Core | Project | Modules |
|----------------|------|---------|---------|
| Runtime host, module registry, services | Yes | Uses, does not fork | Registers via installer |
| Bootstrap runner contract | Yes | Registers project installer | — |
| Project diagnostics ownership | No | Yes | Feature extensions only |
| Gameplay mechanics | No | Skeleton only | Yes |
| Module-owned data | No | No | Yes (inside module) |
| Save/network implementation | No | Contracts only | Planned implementations |

**Never** add gameplay logic under `Assets/CCS/Framework/Core/`.  
**Never** use a global `Assets/CCS/Database/` — data belongs in the owning module.

---

## 3. Module hierarchy

| Layer | Example ID | Notes |
|-------|------------|-------|
| Module prefix | `ccs.survival.` | All survival module IDs |
| Character skeleton | `ccs.survival.character` | First installed module |
| Future gameplay | `ccs.survival.inventory`, etc. | Explicit installer registration |

**Skeleton expectations:** 1 module, 0 services, 0 updatables, 1 bootstrap installer.

Modules inherit `CCS_SurvivalModuleBase`; installers inherit `CCS_SurvivalModuleInstallerBase`.

---

## 4. Validation philosophy

- **When:** Bootstrap and diagnostics only — never per-frame hot paths.
- **Result type:** `CCS_SurvivalValidationResult` (success / warning / fail) → `CCS_Result` at boundaries.
- **Ownership:** Survival utilities validate survival rules; Core validates host/registry contracts.
- **Warnings vs failures:** Skeleton bootstrap fails on missing character module or invalid module ID; optional profile slots and missing avatars **warn or pass**, not fail.
- **DRY:** Identity format checks live in `CCS_SurvivalIdentityUtility`; profile field checks in `CCS_SurvivalProfileValidationUtility`.

---

## 5. Diagnostics philosophy

- **Survival scenes:** `CCS_SurvivalBootstrap` runs `CCS_SurvivalDiagnostics` when enabled.
- **Core diagnostics:** Should be **disabled** on survival scene hosts (warning if enabled).
- **Log categories:** Centralized in `CCS_SurvivalRuntimeConstants` — do not hardcode strings in new scripts.
- **Extension:** Future feature diagnostics add feature-owned categories; call through survival patterns, not Core smoke tests.

---

## 6. Profile philosophy

| Concept | Rule |
|---------|------|
| **Profiles** | `CCS_SurvivalProfileBase` ScriptableObjects — setup/configuration only |
| **profileId** | `ccs.survival.profile.*` — save-stable, not asset path |
| **Runtime state** | Lives in modules/services/context — never in profile assets |
| **Bootstrap slots** | Optional `CCS_SurvivalBootstrapProfileSlot` — setup wiring only |
| **Inheritance** | Future gameplay profiles derive from `CCS_SurvivalProfileBase` in feature assemblies |

**Anti-pattern:** Storing hunger, inventory, or transform data in profile assets as authoritative simulation state.

---

## 7. Authority / avatar philosophy

| Layer | Identity | Persistence |
|-------|----------|-------------|
| **Authority** | `ccs.survival.authority.*` | Save/network ownership keys (future) |
| **Avatar** | `ccs.survival.avatar.*` | Scene representation; links to authority ID |
| **Binding** | `ccs.survival.binding.*` | Optional pair correlation ID |

Authority owns decisions and stable identity. Avatar owns `Transform` root and presence flags. **Never** use GameObject names or instance IDs as save keys.

---

## 8. Scene bootstrap philosophy

See [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md).

- One `CCS_RuntimeHost` + one `CCS_SurvivalBootstrap` on the same composition root.
- One `CCS_SurvivalRuntimeContext` per bootstrap.
- Scene names ≠ save identity.
- Profile slots optional during skeleton phase.

---

## 9. Identity coherence (single rule set)

All stable IDs share:

- Lowercase reverse-DNS: `a-z`, `0-9`, `.`, `-`
- Documented prefix per concept (module, profile, authority, avatar, binding, bootstrap slot)
- **Forbidden:** asset paths, scene paths, slashes, spaces, `GetInstanceID()`, GameObject names

| Identity type | Prefix | Authoritative for save? |
|---------------|--------|-------------------------|
| Module | `ccs.survival.` | Module registration |
| Profile | `ccs.survival.profile.` | Setup + future save schema keys |
| Authority | `ccs.survival.authority.` | Yes (future) |
| Avatar | `ccs.survival.avatar.` | Instance only; authority owns persistence |
| Bootstrap slot | `ccs.survival.bootstrap.slot.` | Setup wiring only |
| Scene / GO name | — | **Never** |

---

## 10. Save-system planning

> Persist **stable string IDs** and serialized gameplay state blobs.  
> Never persist Unity asset paths, scene references, or `Transform` hierarchy paths as primary keys.

Future save adapters (see `CCS_SurvivalFrameworkFutureMarkers.SaveSerializationAdapters`) live in gameplay assemblies.

---

## 11. Multiplayer adaptation (future)

> Networking adapts to **authority** later. Foundation contracts expose `IsNetworkAuthorityReady` without netcode references.

Replication targets avatar representation; ownership decisions flow through authority interfaces in gameplay assemblies.

---

## 12. Future gameplay modules

See [Future_Gameplay_Module_Guidelines.md](Future_Gameplay_Module_Guidelines.md).

- Register through explicit installers.
- Use profiles for tuning, not simulation state.
- Respect bootstrap profile expectations for modules, services, and updatables.

---

## 13. Contributor rules

1. Project → Core dependency only; Modules → Project + Core.
2. No singletons, no scene scan composition, no auto-discovery installers.
3. Use `CCS_SurvivalRuntimeConstants` for log categories and ID prefixes.
4. Add validation in static utilities; run at bootstrap/diagnostics only.
5. Use CCS file headers; no XML `///` summaries.
6. Follow region order from `CCS_Script_Standards.md`.
7. Reference `CCS_SurvivalFrameworkFutureMarkers` for planned integration points.

---

## 14. Anti-patterns

| Anti-pattern | Why forbidden |
|--------------|---------------|
| `FindObjectOfType` for composition | Hidden dependencies; breaks multiplayer-safe design |
| Saving `Assets/...` or scene paths | Breaks builds and content moves |
| GameObject name as player ID | Not stable across sessions |
| Gameplay logic in `CCS_SurvivalProfileBase` | Profiles are configuration only |
| Enabling Core diagnostics in survival scenes | Duplicates/conflicts with survival diagnostics ownership |
| Registering services before module phase requires them | Violates bootstrap profile contract |
| Netcode references in `CCS.Project.Runtime` | Foundation must stay netcode-free |
| Module data in `Shared/` or global `Database/` | Violates module ownership |

---

## Related documents

- [Survival Framework Architecture Gate](Survival_Framework_Architecture_Gate.md)
- [Survival Runtime Foundation](Survival_Runtime_Foundation.md)
- [Survival Validation Standards](Survival_Validation_Standards.md)
- [Survival Authority and Avatar Architecture](Survival_Authority_And_Avatar_Architecture.md)
- [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md)
- [Future Gameplay Module Guidelines](Future_Gameplay_Module_Guidelines.md)
