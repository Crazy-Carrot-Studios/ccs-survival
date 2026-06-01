# CCS Survival — Hotbar / Active Item Module

**Milestone:** 1.2.2 — Active Item Slot + Use Flow Foundation  
**Module ID:** `ccs.survival.hotbar`  
**Namespace:** `CCS.Modules.Hotbar` (editor: `CCS.Modules.Hotbar.Editor`)  
**Author:** James Schilz  
**Date:** 2026-06-01  

---

## Purpose

Provide a **service-driven active item foundation** so the player can select and use one equipped (or future hotbar) item without final hotbar UI, combat animations, or IK.

This is **not** final hotbar UI. It is the runtime selection + use routing layer that future UI and multiplayer authority can build on.

---

## Architecture

```text
CCS_PlayerActiveItemDriver (PrimaryAction / Alpha1)
        ↓
CCS_ActiveItemService
        ↓ behavior routing
CCS_CombatService (weapons) | future tool/consumable handlers
        ↓
Equipment visuals (unchanged — no duplicate spawn)
```

**Rules:**

- Inventory and equipment services remain authoritative for ownership.
- Active item use for weapons routes to `CCS_CombatService.TryMeleeAttack` when the active weapon is equipped in **MainHand**.
- Tools/consumables return a safe `NoBehaviorRegistered` result until dedicated systems exist.
- Equipment visuals from 1.2.0/1.2.1 are reused; active selection does not spawn duplicate meshes.

---

## Folder layout

```text
Assets/CCS/Modules/Hotbar/
  Runtime/
    ActiveItem/     → slot types, state, service, bridge
    Profiles/       → CCS_ActiveItemProfile
    Validation/     → CCS_ActiveItemValidationUtility
  Editor/
    Validation/     → pipeline validator
  Documentation/    → this file

Assets/CCS/Survival/Profiles/Hotbar/
  CCS_DefaultActiveItemProfile.asset
```

---

## Bootstrap

```text
CCS.Survival.Editor.Development.CCS_ActiveItemFoundationBootstrapSetup.ExecuteBatch
CCS.Modules.Playtesting.Editor.CCS_PlaytestBootstrapSetup.ExecuteBatch
```

---

## Playtest (bootstrap harness)

| Key | Action |
|-----|--------|
| **F6** | Equip starter spear |
| **Alpha1** | Select active item from main hand |
| **Primary (LMB)** | Use active item (combat or safe no-target) |

---

## Deferred

- Final hotbar UI and slot keybinds 1–8
- Full animation / IK / aim poses
- Multiplayer authority replication (snapshot types are future-ready)
- Gathering tool routing from active slot (use existing interact/gather flows)
