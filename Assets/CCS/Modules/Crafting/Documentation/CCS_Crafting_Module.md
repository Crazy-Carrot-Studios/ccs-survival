# CCS Survival — Crafting Module

**Milestone:** 0.5.0 — Crafting Module Foundation  
**Module ID:** `ccs.survival.crafting`  
**Namespace:** `CCS.Modules.Crafting` (editor: `CCS.Modules.Crafting.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete (data and service architecture only; not wired to bootstrap installer)

---

## Purpose

Provide the **runtime crafting data and service architecture** used by future Crafting UI, world stations, progression, Save/Load, and HUD notification systems — without implementing those features in 0.5.0.

The module answers:

| Question | Owner |
|----------|--------|
| What is a recipe? | `CCS_CraftingRecipeDefinition` |
| What does a craft consume and produce? | Ingredient/result definitions referencing `CCS_ItemDefinition` |
| What station is required? | `CCS_CraftingStationType` + `CCS_CraftingStationContext` |
| Who executes crafting? | `CCS_CraftingService` |
| What changed? | Crafting events on the service |

---

## Architecture flow

```text
CCS_CraftingRecipeDefinition (ScriptableObject identity + ingredients/results)
        ↓
CCS_CraftingRequest (recipe + station context + craft count)
        ↓
CCS_CraftingService (validate → consume ingredients → grant results)
        ↓
CCS_PlayerInventoryService (HasItem / RemoveItem / AddItem / CanAdd)
        ↓
CCS_CraftingResult + crafting events
```

**Critical rule:** Crafting never references UI, Interaction, world resource harvesting, save/load, or world station MonoBehaviours in 0.5.0.

---

## Folder layout

```text
Assets/CCS/Modules/Crafting/
  Runtime/
    Definitions/    → station type, recipe, ingredient, result definitions
    Data/             → request, result, queue entry, snapshot
    Stations/         → runtime station context (no world objects yet)
    Services/         → CCS_CraftingService
    Events/           → event args + contracts
    Profiles/         → CCS_CraftingProfile
    Validation/       → runtime profile and recipe validation
  Editor/
    Validation/       → pipeline validator + menu
  Documentation/      → this file

Assets/CCS/Survival/Profiles/Crafting/
  CCS_DefaultCraftingProfile.asset
```

---

## Recipe architecture

| Field | Type | Notes |
|-------|------|-------|
| Recipe Id | string | Stable reverse-DNS identity |
| Display Name | string | Player-facing label |
| Description | string | Future UI / tooltips |
| Required Station Type | `CCS_CraftingStationType` | Hand, FirePit, Workbench, Forge, Apothecary |
| Ingredients | list | References `CCS_ItemDefinition` + quantity |
| Results | list | References `CCS_ItemDefinition` + quantity |
| Craft Time Seconds | float | Base duration before profile multiplier |
| Is Unlocked By Default | bool | Available without explicit unlock |

Recipe definitions are **ScriptableObjects**. Validation rejects null ingredient items, null result items, non-positive ingredient quantities, non-positive result quantities, and negative craft times.

---

## Station type architecture

| Station | Purpose |
|---------|---------|
| Hand | Immediate crafting without a world station |
| FirePit | Cooking / heat-based recipes (future world object) |
| Workbench | General assembly recipes |
| Forge | Metalworking recipes |
| Apothecary | Medicine and alchemical recipes |

`CCS_CraftingStationContext` carries station type, display name, and a future `StationId` placeholder. No world MonoBehaviours are created in 0.5.0.

---

## Crafting service flow

1. Receive `CCS_CraftingRequest` with recipe and station context.
2. Validate recipe definition and unlock state.
3. Validate station type against recipe and profile (`AllowHandCrafting` for Hand recipes).
4. Validate inventory ingredients via `CCS_PlayerInventoryService.HasItem`.
5. Validate output capacity via `CCS_PlayerInventoryService.CanAdd`.
6. Remove ingredients with `RemoveItem`.
7. Grant results with `AddItem`.
8. Raise `OnCraftingRequested`, `OnCraftingStarted`, `OnCraftingCompleted`, or `OnCraftingFailed`.

Failures return `CCS_CraftingResult.Failure` — no exceptions for expected validation failures.

Immediate crafting is supported. Timed queue processing is deferred until `AllowQueueing` is implemented.

---

## Inventory dependency

Crafting consumes and produces items exclusively through **`CCS_PlayerInventoryService`**:

| Operation | API |
|-----------|-----|
| Check ingredients | `HasItem(CCS_ItemDefinition, int quantity)` |
| Pre-check output space | `CanAdd(CCS_ItemDefinition, int quantity)` |
| Consume inputs | `RemoveItem(CCS_ItemDefinition, int quantity)` |
| Grant outputs | `AddItem(CCS_ItemDefinition, int quantity)` |

Inventory does not reference Crafting. Composition/bootstrap wiring connects services at runtime.

---

## Profile defaults

Default asset: `Assets/CCS/Survival/Profiles/Crafting/CCS_DefaultCraftingProfile.asset`

| Setting | Default |
|---------|---------|
| Allow hand crafting | true |
| Allow queueing | false |
| Max queue size | 1 |
| Craft time multiplier | 1.0 |

---

## Events

| Event | When |
|-------|------|
| OnCraftingRequested | Craft attempt received |
| OnCraftingStarted | Validation passed; execution begins |
| OnCraftingCompleted | Ingredients consumed and results granted |
| OnCraftingFailed | Validation or execution failure |
| OnRecipeUnlocked | Recipe added to unlocked set |

---

## Deferred systems

| System | Status |
|--------|--------|
| Crafting UI | Not in 0.5.0 |
| World station objects | Not in 0.5.0 |
| Timed craft queue processing | Data shape only |
| Resource harvesting integration | Not in 0.5.0 |
| Save/load recipe unlock persistence | Not in 0.5.0 |
| Bootstrap service registration | Future composition pass |

---

## Validation

**Editor menu:** **CCS → Survival → Crafting → Validate Crafting**

Validator ID: `ccs.survival.validation.crafting`

Registered via `CCS_CraftingValidationRegistration` on the central `CCS_SurvivalValidationPipeline`.

---

## Related docs

- [Inventory Module](../Inventory/Documentation/CCS_Inventory_Module.md)
- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
