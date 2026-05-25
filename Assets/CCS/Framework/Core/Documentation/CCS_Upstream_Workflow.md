# CCS Upstream Workflow

**Version:** 0.4.2  
**Status:** Reusable Core Platform upstream guidance  
**Author:** James Schilz  
**Date:** 2026-05-24

This document describes how **`ccs-framework`** serves as the permanent reusable upstream for Crazy Carrot Studios game projects.

---

## Studio repository structure

| Repository | Role |
|------------|------|
| **ccs-framework** | Reusable upstream Core Platform |
| **ccs-survival** | Survival-focused gameplay and game repo |

Use **genre- or mode-focused** repo names (`ccs-survival`), not a single game title or lore name. Themes such as *Reckoning*, western MMO setting, factions, and narrative belong in the **game project** (product name, scenes, content, module IDs like `ccs.survival.inventory`) — not in `ccs-framework` or upstream documentation examples.

---

## Reusable upstream philosophy

`ccs-framework` is the **engine/platform layer**, not a game project.

| Principle | Rule |
|-----------|------|
| **Upstream = platform** | Runtime host, bootstrap, modules, services, diagnostics, validation, and shared architecture live here |
| **Game repos = gameplay** | Survival, MMO, learning, UI flows, quests, content, and game scenes live in forked game repositories |
| **Manual composition** | No singleton managers, no auto-discovery, no scene scanning |
| **Stable contracts** | `CCS_Result`, module lifecycle, installers, and error codes are the integration surface |

Improvements that benefit **all** future games belong upstream. Features that serve **one** game belong in that game's repository.

---

## Platform code vs gameplay code

| Platform code (upstream) | Gameplay code (game repos) |
|------------------------|----------------------------|
| `CCS_RuntimeHost`, bootstrap runner, update loop | Character controllers tied to a genre |
| Module registry, install plans, dependency metadata | Inventory, crafting, weapons, factions |
| Service registry, event dispatcher | Quest systems, MMO zones, survival loops |
| Diagnostics, validation, error codes | Game-specific UI and HUD |
| Smoke tests under `Core/Runtime/SmokeTests/` | Production game scenes and content |
| Bootstrap validation scene (`SCN_CCS_Bootstrap`) | Levels, biomes, narrative content |

**Rule of thumb:** If removing it would break the *framework* for every game, it belongs upstream. If removing it only breaks *one* game, it does not.

---

## How future game repos should branch or fork

### Option A — GitHub template (recommended for new games)

1. Mark `ccs-framework` as a **Template repository** (see [CCS GitHub Template Setup](CCS_GitHub_Template_Setup.md)).
2. Use **Use this template** to create `ccs-survival`, `ccs-kids-learning`, etc.
3. Rename Unity product/settings for the game; keep `Assets/CCS/Framework/Core/` aligned with upstream or subtree-merge updates.

### Option B — Fork + diverge

1. Fork `ccs-framework` on GitHub.
2. Rename repository and update `README` / `bundleVersion` for the game.
3. Implement gameplay under `Assets/CCS/Modules/` in the fork.
4. Periodically merge or cherry-pick upstream Core fixes.

### Option C — Git submodule / subtree (advanced)

1. Add `ccs-framework` Core path as a submodule or subtree in a game repo.
2. Pin to a tagged upstream baseline (e.g. `v0.4.0-core-platform-baseline`).
3. Bump submodule tag when adopting a new Core release.

---

## Upstreaming reusable improvements

When a fix or feature in a **game repo** should become shared:

1. **Extract** platform-only changes (no game IDs, no genre logic, no content assets).
2. **Open a PR** against `ccs-framework` `main` with a clear milestone message.
3. **Run** `SCN_CCS_Bootstrap` smoke tests with diagnostics enabled.
4. **Bump** patch version in README + `bundleVersion` for upstream releases.
5. **Tag** significant baselines (e.g. `v0.4.x-core-platform-baseline`).

Do **not** upstream: gameplay modules, game scenes, ScriptableObject content, or genre-specific services.

---

## Recommended Git workflow

```text
ccs-framework (upstream main)
  ├── tag: v0.4.0-core-platform-baseline
  ├── patch: 0.4.1, 0.4.2, …
  └── docs-only / platform milestones

ccs-survival (or ccs-<game-name>) (fork/template)
  ├── main — game development
  ├── upstream/core-sync — optional branch to merge ccs-framework
  └── gameplay/ — feature branches per system
```

| Practice | Recommendation |
|----------|----------------|
| **Default branch** | `main` protected on upstream |
| **Commits** | One milestone per commit; no local Unity churn |
| **Pull requests** | Required for upstream; review for Core-only scope |
| **Tags** | Tag frozen baselines; game repos pin tags |

---

## Recommended versioning flow

| Repo | Version meaning |
|------|-----------------|
| **ccs-framework** | `Major.Minor.Patch` — platform milestones (Phase One = 0.4.x) |
| **Game repos** | Independent product version; may embed upstream tag in docs |

Sync on every upstream milestone:

1. `README.md` → Current Framework Version  
2. `ProjectSettings.asset` → `bundleVersion`  
3. Release notes / architecture doc when behavior changes  

---

## Keeping Core clean

Before every upstream commit:

| Check | Expected |
|-------|----------|
| No `.cs` gameplay under `Assets/CCS/Modules/` | Placeholder folders only |
| All Core `.cs` under `Framework/Core/` | Platform + smoke tests only |
| Smoke tests diagnostics-gated | `EnableRuntimeDiagnostics` |
| No singleton / auto-discovery | Instance-owned subsystems |
| Valid 32-char hex `.meta` GUIDs | Unity imports all scripts |
| No incidental scene/settings commits | Bootstrap scene local-only unless intentional |

**Repository validation (0.4.2):** 49 Core `.cs` files (including isolated smoke tests); 0 gameplay `.cs` under `Assets/CCS/Modules/`; meta GUID scan passed; no reflection-based discovery in Core.

---

## Related documents

- [CCS Core Platform Architecture](CCS_Core_Platform_Architecture.md)
- [CCS GitHub Template Setup](CCS_GitHub_Template_Setup.md)
- [CCS Phase One Completion Checklist](CCS_Phase_One_Core_Platform_Completion_Checklist.md)
- [CCS Core Template Readiness Checklist](CCS_Core_Template_Readiness_Checklist.md)
