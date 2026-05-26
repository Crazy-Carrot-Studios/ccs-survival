# Milestone 0.3.4 — Survival Scene Bootstrap Standards

**Version:** 0.3.4  
**Status:** Foundation milestone (no gameplay)  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Predecessor:** [Milestone 0.3.3](Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md) (`v0.3.3`)

**Goal:** Survival-owned scene bootstrap standards and validation helpers so all future survival scenes use a consistent AAA-safe composition root.

---

## Purpose

Before gameplay systems begin, every survival scene needs a predictable startup pattern:

- One runtime host
- One survival bootstrap
- One survival runtime context
- Validated diagnostics ownership
- No accidental duplicate installers
- No hidden service/updatable registration during skeleton phase
- Future profile-driven setup compatibility

---

## Scene startup rules

| Rule | Detail |
|------|--------|
| Single host | `ExpectedRuntimeHostCount` = 1 |
| Single bootstrap | `ExpectedSurvivalBootstrapCount` = 1 on same root as host |
| Composition root | `CCS_SurvivalBootstrap` drives context, installers, diagnostics |
| Diagnostics ownership | Survival scenes use survival diagnostics; Core diagnostics off |
| Skeleton registry | Modules=1, Services=0, Update systems=0, BootstrapInstallers=1 |
| Validation timing | Bootstrap/diagnostics only — no per-frame scene scanning |

---

## What was added

| File | Role |
|------|------|
| `CCS_SurvivalSceneBootstrapRules` | Static scene bootstrap rule constants |
| `CCS_SurvivalSceneBootstrapValidationUtility` | Runtime scene bootstrap validation (uses `CCS_SurvivalValidationResult`) |
| `CCS_SurvivalBootstrapProfileSlot` | Serializable future profile slot placeholder |
| `Scene_Bootstrap_Standards.md` | Authoring guide for survival scenes |

**Updated:** `CCS_SurvivalRuntimeConstants`, `CCS_SurvivalBootstrap` (optional empty profile slots), `CCS_SurvivalDiagnostics`.

**Not added:** `CCS_SurvivalSceneValidationResult` — existing `CCS_SurvivalValidationResult` is sufficient.

---

## Scene identity is not save identity

> Scene names, hierarchy paths, and GameObject names must not be used as save or authority identity.

Use module, authority, profile, and bootstrap slot IDs instead.

---

## Profile slots are setup/config only

- Optional serialized slots on `CCS_SurvivalBootstrap` (default empty).
- `slotId` uses `ccs.survival.bootstrap.slot.*` prefix.
- Validated when assigned; **empty slots do not fail** skeleton bootstrap.
- No Resources, Addressables, or save IO.

---

## How future scenes should be composed

1. Add `PF_CCS_Survival_BootstrapRoot` (or equivalent) with `CCS_RuntimeHost` + `CCS_SurvivalBootstrap`.
2. Disable Core runtime diagnostics on the host.
3. Enable survival diagnostics on bootstrap (default).
4. Register survival installers only through the bootstrap pipeline.
5. Add optional profile slots when a feature milestone introduces setup profiles.

---

## What was not added

- No gameplay, movement, input, controller, inventory, attributes, combat, AI
- No save system, networking package, or new scene content requirements
- No services or updatables registered
- No Core modifications

---

## Related documents

- [Scene Bootstrap Standards](../Scene_Bootstrap_Standards.md)
- [Milestone 0.3.3](Milestone_0.3.3_Survival_Authority_Avatar_Boundary_Skeleton.md)
