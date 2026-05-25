# Milestone 0.1.0 — Survival Project Identity Setup

**Version:** 0.1.0  
**Status:** Documentation milestone  
**Author:** James Schilz  
**Date:** 2026-05-24

**Goal:** Make **ccs-survival** a gameplay-focused repository while keeping reusable Core protected.

---

## Scope

### In scope

- [x] Repo-root `README.md` reflects **ccs-survival** (not generic framework template)
- [x] `Documentation/Architecture/` survival direction docs
- [x] `Documentation/Milestones/` milestone tracking
- [x] `Assets/CCS/Survival/` folder identity (`Documentation/`, `Scripts/` reserved)
- [x] Architectural boundaries: Core vs Modules vs Survival shell
- [x] Networking authority **direction** (no packages)
- [x] Persistence **direction** (no save code)

### Explicitly out of scope

- [ ] No gameplay systems or `CCS_IModule` implementations
- [ ] No inventory, character controller, crafting, or equipment code changes
- [ ] No networking package install
- [ ] No persistence / save runtime code
- [ ] No new gameplay scripts under `Assets/CCS/Survival/Scripts/`
- [ ] No Core Platform behavior changes under `Assets/CCS/Framework/Core/`

---

## Deliverables

| Artifact | Path |
|----------|------|
| Project README | `README.md` |
| Gameplay architecture | `Documentation/Architecture/Survival_Gameplay_Architecture.md` |
| Module boundaries | `Documentation/Architecture/Survival_Module_Boundaries.md` |
| Networking authority | `Documentation/Architecture/Survival_Networking_Authority.md` |
| Persistence direction | `Documentation/Architecture/Survival_Persistence_Direction.md` |
| This milestone | `Documentation/Milestones/Milestone_0.1.0_Survival_Project_Identity_Setup.md` |
| Unity-side doc index | `Assets/CCS/Survival/Documentation/README.md` |

---

## Folder structure created

```text
Documentation/
  Architecture/
  Milestones/
Assets/CCS/Survival/
  Documentation/
  Scripts/          # empty — reserved for later milestones
```

---

## Verification checklist

Run from repo root:

```powershell
cd "C:\Users\james\OneDrive\Documents\GitHub\ccs-survival"
git status
```

**Expected:** New documentation and `Assets/CCS/Survival/` paths; optional pre-existing `ProjectSettings` churn should not be committed unless intentional.

**Manual review:**

1. `README.md` title is **CCS Survival** and links architecture docs.
2. No new `.cs` gameplay files outside Core (existing Core unchanged).
3. `Assets/CCS/Framework/Core/` has no survival-specific edits in this milestone.
4. Module folders under `Assets/CCS/Modules/` remain scaffolding only.

**Core smoke (unchanged):** Play `SCN_CCS_Bootstrap` with diagnostics enabled — same three smoke log lines as Core baseline.

---

## Success criteria

- A new contributor can read repo docs and know **where gameplay lives** vs **what is protected Core**
- Module ID convention `ccs.survival.*` is documented
- Multiplayer and save direction are written before implementation
- Milestone can be tagged `v0.1.0-survival-identity` when approved (optional)

---

## Completed successor

[Milestone 0.2.0 — Survival Bootstrap Scene + Empty Install Pipeline](Milestone_0.2.0_Survival_Bootstrap_Scene_Empty_Install_Pipeline.md) (`v0.2.0-survival-bootstrap` when tagged).

## Suggested next milestone (0.3.0 — not started)

1. First module skeleton with installer registered from `CCS_SurvivalInstaller` (`ccs.survival.character` or agreed priority)
2. Documented manual module install order in survival installer
3. Align `ProjectSettings` bundle version with game milestone tagging policy

---

## Related documents

- [Survival Gameplay Architecture](../Architecture/Survival_Gameplay_Architecture.md)
- [Survival Module Boundaries](../Architecture/Survival_Module_Boundaries.md)
- [CCS Upstream Workflow](../../Assets/CCS/Framework/Core/Documentation/CCS_Upstream_Workflow.md)
