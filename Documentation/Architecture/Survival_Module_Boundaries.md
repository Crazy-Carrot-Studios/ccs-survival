# Survival Module Boundaries

**Version:** 0.2.0  
**Status:** Boundary rules (enforcement via code review and project standards)  
**Author:** James Schilz  
**Date:** 2026-05-24

Defines what code and assets live in **Core**, **Modules**, **Project**, and **Shared** — and what must never cross those lines.

---

## Boundary summary

| Zone | Path | May contain | Must not contain |
|------|------|-------------|------------------|
| **Core Platform** | `Assets/CCS/Framework/Core/` | Runtime host, bootstrap, module contracts, registry, smoke tests | Survival mechanics, quests, factions, genre UI |
| **Framework (non-Core)** | `Assets/CCS/Framework/` (outside Core) | Shared framework docs, future shared non-game utilities | Game-specific modules |
| **Gameplay modules** | `Assets/CCS/Modules/<Feature>/` | Installers, modules, runtime, editor, tests, prefabs, **module-owned data** | Changes to Core types; singleton globals |
| **Project shell** | `Assets/CCS/Project/` | Bootstrap, composition, scenes, project docs, install sequencing | Heavy feature logic (belongs in a module) |
| **Shared** | `Assets/CCS/Shared/` | Cross-module assets used by 2+ modules | Module-specific content; bootstrap roots |
| **Tests** | `Assets/CCS/Tests/` | Cross-cutting test harnesses and results | Feature-specific tests (belong in module) |
| **Repo documentation** | `Documentation/` | Architecture, milestones, studio-facing plans | Generated Unity cache |

---

## Core Platform — protected surface

Treat `Assets/CCS/Framework/Core/` as **read-mostly** from a gameplay perspective.

### Allowed in Core (upstream only)

- Bug fixes and contract improvements that benefit all games
- New generic capabilities (e.g. shared save **interface** without survival item schema)

### Forbidden in Core

- References to `ccs.survival.*` module IDs in Core runtime code
- Hardcoded western, faction, or *Reckoning* assumptions
- Inventory stacks, damage formulas, or biome generators
- Game HUD or menu flows

When Core needs a hook, add a **generic contract** upstream; implement survival behavior in **Modules**.

---

## Gameplay modules — feature ownership

Each folder under `Assets/CCS/Modules/` should map to **one** module ID and one installer.

### Standard module layout (target)

```text
Assets/CCS/Modules/Inventory/
  Runtime/          # CCS_IModule implementation, systems
  Editor/           # Inspectors, tooling (no runtime dependency)
  Tests/            # Edit Mode / Play Mode tests for this feature
  Prefabs/          # Feature prefabs
  Settings/         # ScriptableObject config
  UI/               # Feature UI (game-specific)
```

### Module rules

1. **One module ID per feature folder** (e.g. `ccs.survival.inventory`).
2. **Installer** extends `CCS_ModuleInstallerBase`; registers module with host `ModuleHost`.
3. **Services** exposed via interfaces registered on `CCS_ServiceRegistry` during install.
4. **No** `FindObjectOfType` / scene scan for other modules.
5. **No** static `Instance` service locators.

### Existing module folders (placeholder)

The repo may contain module **folder scaffolding** without runtime scripts. At milestone **0.1.0**, do not add or modify gameplay code in:

- `CharacterController`
- `Inventory`
- `Crafting`
- `Equipment`
- `Hotbar`
- `SaveSystem`
- `UI`

Implementation resumes in later milestones with explicit scope.

---

## Project shell — lightweight composition scope

`Assets/CCS/Project/` holds material that applies to the **whole game** but is not a pluggable Core module:

- Bootstrap scene (`SCN_CCS_Survival_Bootstrap`) and prefab (`PF_CCS_Survival_BootstrapRoot`)
- `CCS.Project.Runtime` startup shell (`CCS_SurvivalBootstrap`, installer, diagnostics, runtime context)
- Project documentation index
- Future: bootstrap install plan asset, game version constants

Do **not** turn Project into a “god module” that absorbs inventory, crafting, or networking. Those remain separate modules under `Assets/CCS/Modules/`.

**No global `Database/` folder.** Module-owned data (items, recipes, loot) lives inside the owning module.

### Assembly dependency rule

| Assembly | May reference |
|----------|----------------|
| `CCS.Core.Runtime` | Nothing game-specific |
| `CCS.Project.Runtime` | `CCS.Core.Runtime` only |
| `CCS.Modules.<Feature>.Runtime` | `CCS.Core.Runtime`, optionally `CCS.Project.Runtime` |

**Never** reference Project or Modules from Core.

---

## Upstreaming decision tree

```text
Change needed?
  ├─ Would every CCS game use it? → PR to ccs-framework
  ├─ Only survival genre? → Module in ccs-survival
  └─ Only one product theme asset? → Content under game scenes / art paths
```

---

## Dependency metadata vs install order

| Mechanism | Role |
|-----------|------|
| `CCS_IModuleDependencyProvider` | Declares IDs for **validation** before install |
| Manual install plan | **Authoritative** order; no auto-sort |

Mismatch between declared dependencies and install plan is a **build/bootstrap error**, not a runtime auto-fix.

---

## Tests and smoke

| Test type | Location |
|-----------|----------|
| Core platform smoke | `Framework/Core/Runtime/SmokeTests/` — diagnostics gated |
| Module unit/play tests | `Assets/CCS/Modules/<Feature>/Tests/` |
| Full game integration | Future game test scene (not Core bootstrap) |

Gameplay tests must not require modifying `SCN_CCS_Bootstrap` or Core smoke installers.

---

## Anti-patterns (reject in review)

- Singleton `GameManager` or static `Inventory.Instance`
- Scene-wide installer discovery
- Gameplay `#if` branches inside Core files
- Module ID `ccs.reckoning.*` in code shared with upstream docs

---

## Related documents

- [Survival Gameplay Architecture](Survival_Gameplay_Architecture.md)
- [CCS Core Platform Architecture](../../Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md)
- [CCS Upstream Workflow](../../Assets/CCS/Framework/Core/Documentation/CCS_Upstream_Workflow.md)
