# CCS GitHub Template Setup

**Version:** 0.4.2  
**Status:** Documentation only — no automatic GitHub changes  
**Author:** James Schilz  
**Date:** 2026-05-24

Use this guide when preparing **`ccs-framework`** as a GitHub **Template repository** for new Crazy Carrot Studios games.

> **Note:** These steps are performed manually in GitHub and Unity. This document does not modify repository settings automatically.

---

## Mark as Template Repository

1. Open **GitHub** → `Crazy-Carrot-Studios/ccs-framework` → **Settings**.
2. Under **General**, enable **Template repository**.
3. Save.

New projects can use **Use this template** → **Create a new repository** without copying fork history (unless you choose to fork instead).

---

## Recommended branch protection (`main`)

| Setting | Recommendation |
|---------|----------------|
| **Require pull request** | Before merging to `main` |
| **Require status checks** | When CI is added later |
| **Restrict force push** | Enabled |
| **Restrict deletion** | Enabled |
| **Include administrators** | Optional for solo maintainer phase |

Allow direct pushes only during early solo development if needed; tighten before team scale-up.

---

## Recommended tags and releases

| Item | Purpose |
|------|---------|
| **`v0.4.0-core-platform-baseline`** | Frozen Phase One Core Platform tag (already published) |
| **Future `v0.4.x-…` tags** | Documented platform milestones |
| **GitHub Releases** | Attach release notes from `Documentation/Releases/` |

**Release process:**

1. Complete Play Mode smoke validation (`SCN_CCS_Bootstrap`, diagnostics on).
2. Bump `README` + `bundleVersion`.
3. Commit milestone on `main`.
4. Create annotated tag: `git tag -a v0.4.x-description -m "…"`.
5. Push branch and tag: `git push origin main --tags`.
6. Publish GitHub Release from tag; paste release notes markdown.

---

## Recommended naming conventions

| Item | Convention | Example |
|------|------------|---------|
| **Upstream repo** | `ccs-framework` | `Crazy-Carrot-Studios/ccs-framework` |
| **Game repos** | `ccs-<genre-or-mode>` | `ccs-survival` |
| **Tags** | `v<semver>-<short-description>` | `v0.4.0-core-platform-baseline` |
| **Commits** | `Milestone X.Y.Z - …` / `Patch X.Y.Z - …` | `Milestone 0.4.2 - Establish reusable core upstream repository` |
| **Module IDs** (games) | Reverse-DNS style | `ccs.survival.inventory` |
| **Core scripts** | `CCS_<Name>` | `CCS_RuntimeHost` |

---

## Recommended clone / fork workflow

### Maintain upstream only

```bash
git clone https://github.com/Crazy-Carrot-Studios/ccs-framework.git
cd ccs-framework
```

### Start a new game from template

1. GitHub → **Use this template** → create `ccs-survival` (or another `ccs-<genre>` repo).
2. Clone the new repository.
3. Open in Unity 6 (match `ProjectSettings/ProjectVersion.txt`).
4. Add gameplay under `Assets/CCS/Modules/` — **not** under `Framework/Core/`.
5. Keep game lore and theme (e.g. a specific western MMO product) in game content and naming inside that repo — not in `ccs-framework`.

### Pull upstream fixes into a game repo

- **Merge:** merge tagged upstream release into game `main` or `upstream-sync` branch.
- **Cherry-pick:** platform fixes only.
- **Avoid:** copying entire `Library/` or `UserSettings/`.

---

## Recommended Unity ignore rules

Ensure `.gitignore` excludes (already standard for this repo):

| Path | Reason |
|------|--------|
| `Library/` | Unity cache |
| `Temp/`, `Obj/`, `Logs/` | Build artifacts |
| `UserSettings/` | Local editor state |
| `*.csproj`, `*.sln` | Generated IDE projects (if policy excludes them) |

**Keep tracked:**

- `Assets/**/*.meta`
- `ProjectSettings/` (except local-only churn)
- `Packages/manifest.json`
- All `Framework/Core/` sources and documentation

**Do not commit:**

- Incidental edits to `SCN_CCS_Bootstrap.unity` unless intentional
- `Mobile_RPAsset.asset` or other local render pipeline tweaks
- `ProjectSettings/SceneTemplateSettings.json` if generated locally

---

## Recommended release process (checklist)

- [ ] Version synced (`README` + `bundleVersion`)
- [ ] No gameplay `.cs` in Core
- [ ] Smoke tests pass with diagnostics enabled
- [ ] No invalid `.meta` GUIDs in Core
- [ ] Working tree clean (no accidental scene churn)
- [ ] Milestone commit on `main`
- [ ] Tag + GitHub Release (for baselines)
- [ ] Update [CCS Upstream Workflow](CCS_Upstream_Workflow.md) if process changes

---

## Related documents

- [CCS Upstream Workflow](CCS_Upstream_Workflow.md)
- [CCS Core Platform Architecture](CCS_Core_Platform_Architecture.md)
- [CCS Phase One Completion Checklist](CCS_Phase_One_Core_Platform_Completion_Checklist.md)
