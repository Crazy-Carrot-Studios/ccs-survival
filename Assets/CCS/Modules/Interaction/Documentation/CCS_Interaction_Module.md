# CCS Survival — Interaction Module

**Milestone:** 0.3.9 — Interaction Module Foundation  
**Module ID:** `ccs.survival.interaction`  
**Namespace:** `CCS.Modules.Interaction` (editor: `CCS.Modules.Interaction.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete (not wired to bootstrap installer or scene prefab)

---

## Purpose

Provide a **reusable interaction framework** used by future Inventory, Crafting, Storage, Building, Quests, Resources, and NPC systems — without implementing any of those features in 0.3.9.

The module answers:

| Question | Owner |
|----------|--------|
| What can I interact with? | Scanner + `CCS_IInteractable` |
| Can I interact with it? | `CCS_IInteractable.CanInteract()` |
| What text should be shown? | `CCS_IInteractable.GetInteractionDisplayName()` |
| What happens on Interact? | Target's `Interact()` implementation |

---

## Architecture flow

```text
Character Controller (scan origin + forward)
        ↓
CCS_InteractionScanner  (forward raycast)
        ↓
CCS_InteractionService.CurrentTarget
        ↓
RequestInteraction()
        ↓
CCS_IInteractable.Interact()   ← target decides gameplay
```

**Critical rule:** Interaction never references Inventory, Crafting, Equipment, Save, or Quest systems. It only detects targets and forwards requests.

---

## Folder layout

```text
Assets/CCS/Modules/Interaction/
  Runtime/
    Detection/      → scanner, detection result
    Interaction/    → service, interactable base
    Interfaces/     → CCS_IInteractable
    Events/         → event args + contracts
    Profiles/       → CCS_InteractionProfile
    Validation/     → runtime profile validation
  Editor/
    Validation/     → pipeline validator + menu
  Documentation/    → this file

Assets/CCS/Survival/Profiles/Interaction/
  CCS_DefaultInteractionProfile.asset
```

---

## Core types

| Type | Role |
|------|------|
| `CCS_IInteractable` | Contract for all interactable world objects |
| `CCS_InteractableBase` | MonoBehaviour base for doors, chests, stations, nodes, NPCs, etc. |
| `CCS_InteractionScanner` | Profile-driven forward raycast |
| `CCS_InteractionService` | Current target, scan tick, interaction requests, events |
| `CCS_InteractionProfile` | Distance + layer mask tuning |

---

## Detection (0.3.9)

- **Forward raycast only** — simple, predictable, cheap
- Profile: **3.0m** default distance, serialized `LayerMask`
- Respects per-target `GetInteractionDistance()` cap

**Deferred:** spherecast, focus assist, gamepad aim assist

---

## Events

| Event | When |
|-------|------|
| `InteractableFound` | Scanner focuses a new target |
| `InteractableLost` | Previous target no longer detected |
| `InteractionRequested` | Player/system requests interaction |
| `InteractionSucceeded` | Target accepted and `Interact()` ran |
| `InteractionFailed` | No target, not allowed, or service not ready |

Event name constants: `CCS_InteractionEvents`.

---

## Runtime service flow

1. `CCS_InteractionService.InitializeFromProfile(CCS_InteractionProfile)`
2. Each frame: `TickScan(scanOrigin, scanForward)` from character/camera
3. UI (future) reads `CurrentTarget.GetInteractionDisplayName()`
4. Input (future) calls `RequestInteraction()`

---

## Validation

| Menu | Path |
|------|------|
| Validate Interaction | **CCS → Survival → Interaction → Validate Interaction** |

Batch entry: `CCS.Modules.Interaction.Editor.CCS_InteractionValidationMenu.ValidateInteraction`

---

## Assemblies

| Assembly | References |
|----------|------------|
| `CCS.Modules.Interaction.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |
| `CCS.Modules.Interaction.Editor` | Core, Survival runtime/editor, Interaction runtime |

---

## Deferred (post-0.3.9)

| Feature | Notes |
|---------|--------|
| Inventory / Crafting / Storage | Implement `CCS_IInteractable` in those modules |
| Interaction UI prompt | Subscribe to Found/Lost events |
| New Input System Interact action | Calls `RequestInteraction()` |
| Bootstrap installer + registry | Manual install plan |
| Spherecast / aim assist | Scanner extensions |
| Doors, chests, workbenches | Subclasses of `CCS_InteractableBase` |

---

## Default profile (0.3.9)

| Setting | Value |
|---------|-------|
| Interaction distance | 3.0 |
| Interaction layers | All layers (`~0`) |
