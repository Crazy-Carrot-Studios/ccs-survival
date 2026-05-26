# Milestone 0.3.3 — Survival Authority Avatar Boundary Skeleton

**Version:** 0.3.3  
**Status:** Foundation milestone (no gameplay)  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Predecessor:** [Milestone 0.3.2](Milestone_0.3.2_Survival_Module_Validation_Diagnostics_Rules.md) (`v0.3.2`)

**Goal:** Establish the survival-owned boundary between player **authority** (ownership, identity, future input/save/network signals) and **avatar** (scene representation) without movement, input, networking, or save implementation.

---

## Purpose

AAA survival/MMO architecture needs a clean split so future systems do not hard-couple:

- Save data and player identity
- Controller logic and camera rigs
- Network ownership and scene bodies

This milestone adds **interfaces and identity validation only**.

---

## Authority vs Avatar

| Layer | Role | Examples (future) |
|-------|------|-------------------|
| **Authority** (`CCS_ISurvivalAuthority`) | Who owns decisions, stable identity, input intent, save keys, network ownership signals | Local player authority, NPC authority stub, host-owned authority |
| **Avatar** (`CCS_ISurvivalAvatar`) | Physical scene representation | Body root, visuals, animator, controller attachment point, camera target, equipment sockets, spawn presence |

**Binding:** `CCS_SurvivalAuthorityAvatarBinding` links `AuthorityId` ↔ `AvatarId` (optional `BindingId`) without spawn or save IO.

---

## What was added

| File | Role |
|------|------|
| `CCS_ISurvivalAuthority` | Authority ownership contract (future-facing properties) |
| `CCS_ISurvivalAvatar` | Scene avatar contract (`Transform` root, spawn/possession flags) |
| `CCS_SurvivalIdentityUtility` | Save-stable ID validation for authority, avatar, profile, binding |
| `CCS_SurvivalAuthorityAvatarBinding` | Readonly authority–avatar relationship value |
| `CCS_SurvivalAuthorityAvatarValidationUtility` | Authority/avatar/binding/match validation |

**Updated:** `CCS_SurvivalRuntimeConstants`, `CCS_SurvivalDiagnostics` (informational notes only; skeleton bootstrap does not require instances).

---

## Identity validation rules

| Rule | Detail |
|------|--------|
| Non-empty | Authority, avatar, binding IDs must not be null/whitespace |
| Prefix | `ccs.survival.authority.*`, `ccs.survival.avatar.*`, `ccs.survival.binding.*` |
| Format | Lowercase reverse-DNS (`a-z`, `0-9`, `.`, `-`) |
| Forbidden sources | Unity instance IDs, GameObject names, scene paths, asset paths, slashes, spaces |

---

## Save-system planning rule

> **Save identity belongs to authority and profile IDs**, not scene objects.  
> Do not persist `Transform`, GameObject names, scene paths, or asset paths as authoritative keys.

---

## Multiplayer planning rule

> Networking will adapt to **authority** later.  
> Authority contracts do **not** depend on a networking package today (`IsNetworkAuthorityReady` is a future-facing signal only).

---

## Avatar rule

> **Avatar is scene representation only** — not persistent ownership identity.  
> `AvatarId` identifies an avatar instance; `AuthorityId` links it to the owning authority.

---

## Systems protected by this split (future)

- Player controller and input routing → authority intent, not avatar mesh
- Save/load → authority/profile IDs
- Camera rigs → avatar root/targets
- Equipment visuals → avatar sockets
- Replication → authority ownership; avatar as replicated view

---

## What was not added

- No movement, input, player controller, networking package, save implementation
- No inventory, attributes, combat, AI, animator, or equipment
- No runtime authority or avatar instances required for skeleton bootstrap
- No services or updatables registered
- No Core modifications

---

## Related documents

- [Survival README](../../README.md)
- [Milestone 0.3.2](Milestone_0.3.2_Survival_Module_Validation_Diagnostics_Rules.md)
