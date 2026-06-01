# CCS Survival — Hotbar / Active Item Module

**Milestone:** 1.2.3 — Primitive Tool Use Routing Foundation  
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
CCS_PlayerActiveItemDriver (PrimaryAction / Alpha1 / Alpha2)
        ↓
CCS_ActiveItemService
        ↓ behavior routing
CCS_CombatService (weapons)
CCS_GatheringService (hatchet/axe → tree/bush, pick → rock)
CCS_HarvestableResource + inventory (world resource harvest)
        ↓
Equipment visuals (unchanged — no duplicate spawn)
```

**Target detection:** `CCS_InteractionService.CurrentTarget` via `CCS_ActiveItemTargetResolver` (no duplicate forward-ray targeting system).

**Rules:**

- Inventory and equipment services remain authoritative for ownership.
- Weapons route to `CCS_CombatService.TryMeleeAttack` when equipped in **MainHand**.
- Tools validate against `CCS_ItemGameplayUtility` / gathering node type before `TryGatherNode`.
- Wrong tool → `WrongTool`; no focused target → `NoTarget`; out of range → `TargetOutOfRange`.
- Consumables and placeables return `NoBehaviorRegistered` until dedicated services exist.
- Equipment visuals from 1.2.0/1.2.1 are reused; active selection does not spawn duplicate meshes.

---

## Result types (1.2.3)

| Type | Meaning |
|------|---------|
| `GatheringSuccess` / `GatheringFailed` | Gathering node attempt |
| `ResourceHarvestSuccess` / `ResourceHarvestFailed` | World resource harvest attempt |
| `WrongTool` | Tool metadata does not match target |
| `TargetOutOfRange` | Interactable beyond interaction distance |
| `TargetUnavailable` | Node depleted or not gatherable |
| `ToolNotEquipped` | Active tool not in main hand / tool / off hand |

---

## Folder layout

```text
Assets/CCS/Modules/Hotbar/
  Runtime/
    ActiveItem/     → slot types, state, service, target resolver, tool utility
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
CCS.Survival.Editor.Development.CCS_ActiveItemToolRoutingBootstrapSetup.ExecuteBatch
CCS.Modules.Playtesting.Editor.CCS_PlaytestBootstrapSetup.ExecuteBatch
```

---

## Playtest (bootstrap harness)

| Key | Action |
|-----|--------|
| **F6** | Equip starter spear |
| **Shift+F6** | Equip bone hatchet (or pick when pick step active) |
| **Alpha1** | Select active item from main hand |
| **Alpha2** | Select active item from tool slot |
| **Primary (LMB)** | Use active item (combat, gather, or safe failure) |

---

## Deferred

- Final hotbar UI and slot keybinds 1–8
- Full animation / IK / aim poses
- Multiplayer authority replication (snapshot types are future-ready)
- Consumable and placeable active use routing
