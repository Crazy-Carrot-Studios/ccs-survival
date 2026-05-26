# Milestone 0.3.2 — Survival Module Validation + Diagnostics Rules

**Version:** 0.3.2  
**Status:** Foundation milestone (no gameplay)  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Predecessor:** [Milestone 0.3.1](Milestone_0.3.1_Survival_Runtime_Foundation_Base_Layer.md) (`v0.3.1`)

**Goal:** Survival-owned validation and diagnostics rules for consistent modules, profile-ready setup, and save-friendly identity planning.

---

## What was added

| File | Role |
|------|------|
| `CCS_SurvivalValidationResult` | Lightweight outcome (success / warning / fail) mapping to `CCS_Result` |
| `CCS_SurvivalModuleValidationUtility` | Module ID, registry, skeleton service/update checks |
| `CCS_SurvivalProfileBase` | Abstract ScriptableObject profile contract (no gameplay data) |
| `CCS_SurvivalProfileValidationUtility` | Profile ID and save-stable identity validation |

**Updated:** `CCS_SurvivalRuntimeConstants`, `CCS_SurvivalDiagnostics`, character module/installer skeleton hooks.

---

## Validation rules (skeleton phase)

| Rule | Expectation |
|------|-------------|
| Module ID prefix | Must start with `ccs.survival.` |
| Module count | `ExpectedSkeletonModuleCount` (= 1) |
| Services | `SkeletonExpectedServicesCount` (= 0) |
| Update systems | `SkeletonExpectedUpdateSystemsCount` (= 0) |
| Duplicate module IDs | None in registry |
| Bootstrap installers | 1 on runner (diagnostics warning) |
| Character module | Installed with valid ID |

---

## Profile-driven setup direction

`CCS_SurvivalProfileBase` is a **configuration contract only** — not gameplay state.

| Concept | Rule |
|---------|------|
| **Profiles** | ScriptableObject setup assets (editor/content) |
| **Runtime state** | Lives in modules/services — separate from profiles |
| **profileId** | Stable reverse-DNS ID (`ccs.survival.profile.*`) for future save compatibility |
| **Forbidden identity** | Unity asset paths, scene references, `GetInstanceID()` |

Future modules may reference profiles for tuning; saved games store **stable IDs**, not profile asset paths.

---

## Save-system planning rule

> Runtime and save identity must use stable string IDs (module IDs, profile IDs, item IDs).  
> Never persist Unity asset paths or scene object references as authoritative keys.

---

## What was not added

- No inventory, attributes, hunger/thirst, combat, AI, save implementation, networking
- No services or updatables registered
- No gameplay profile assets with tuning data
- No Core modifications

---

## Related documents

- [Survival README](../README.md)
- [Milestone 0.3.1](Milestone_0.3.1_Survival_Runtime_Foundation_Base_Layer.md)
