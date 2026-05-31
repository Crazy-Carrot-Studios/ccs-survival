# CCS Survival — Crafting Module

**Milestone:** 0.5.3 — Crafting Gameplay Integration  
**Module ID:** `ccs.survival.crafting`  
**Namespace:** `CCS.Modules.Crafting` (editor: `CCS.Modules.Crafting.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete at **0.5.0**. Gameplay integration complete at **0.5.3**.

---

## Purpose

Provide the **runtime crafting data and service architecture** used by Crafting UI, world stations, progression, Save/Load, and HUD notification systems. **0.5.3** wires crafting into the bootstrap harvest loop without final crafting UI or world station objects.

The module answers:

| Question | Owner |
|----------|--------|
| What is a recipe? | `CCS_CraftingRecipeDefinition` |
| What does a craft consume and produce? | Ingredient/result definitions referencing `CCS_ItemDefinition` |
| What station is required? | `CCS_CraftingStationType` + `CCS_CraftingStationContext` |
| Who executes crafting? | `CCS_CraftingService` |
| What changed? | Crafting events on the service |

---

## Gameplay loop (0.5.3)

```text
Harvest resources (World Resources)
        ↓
CCS_PlayerInventoryService receives Wood / Stone / Fiber
        ↓
CCS_CraftingTestHarness (dev) or future player input calls TryCraft
        ↓
CCS_CraftingService validates ingredients and output capacity
        ↓
Remove ingredients → grant results (rollback on grant failure)
        ↓
Inventory events + crafting events
        ↓
CCS_HudPresentationService refreshes inventory summary + notification queue
```

**Critical rule:** Crafting never references UI, Interaction, world resource harvesting, save/load, or world station MonoBehaviours directly. Composition and HUD wiring connect services at runtime.

---

## Folder layout

```text
Assets/CCS/Modules/Crafting/
  Runtime/
    Definitions/    → station type, recipe, ingredient, result definitions
    Data/             → request, result, queue entry, snapshot
    Stations/         → runtime station context (no world objects yet)
    Services/         → CCS_CraftingService, CCS_CraftingRuntimeBridge
    Testing/          → CCS_CraftingTestHarness (development only)
    Events/           → event args + contracts
    Profiles/         → CCS_CraftingProfile
    Validation/       → runtime profile and recipe validation
  Editor/
    Validation/       → pipeline validator, bootstrap setup, menu
  Documentation/      → this file

Assets/CCS/Survival/Profiles/Crafting/
  CCS_DefaultCraftingProfile.asset
  TestItems/          → Campfire Kit, Bandage
  TestRecipes/        → CCS_TestCampfireRecipe, CCS_TestBandageRecipe
```

---

## Test recipe assets (0.5.3)

| Asset | Station | Ingredients | Result |
|-------|---------|-------------|--------|
| `CCS_TestBandageRecipe` | Hand | Fiber x2 | Bandage x1 |
| `CCS_TestCampfireRecipe` | Hand | Wood x3, Stone x2 | Campfire Kit x1 |

Both recipes use immediate craft time (`0` seconds) and `IsUnlockedByDefault = true`. Ingredient items reuse world resource test items (Wood, Stone, Fiber).

Test output items:

| Asset | Item ID |
|-------|---------|
| `CCS_TestItem_Bandage` | `ccs.survival.item.test.bandage` |
| `CCS_TestItem_CampfireKit` | `ccs.survival.item.test.campfirekit` |

Bootstrap setup batch: `CCS.Modules.Crafting.Editor.CCS_CraftingBootstrapSetup.ExecuteBatch`

---

## Primitive hand recipes (0.9.1)

Early-game recipes under `Assets/CCS/Survival/Profiles/Crafting/PrimitiveRecipes/`:

| Asset | Station | Ingredients | Result |
|-------|---------|-------------|--------|
| `CCS_PrimitiveSpearRecipe` | Hand | Branch x3 | Spear x1 |
| `CCS_PrimitiveBowStaveRecipe` | Hand | Branch x2 | Bow Stave x1 |
| `CCS_PrimitiveArrowShaftRecipe` | Hand | Branch x1 | Arrow Shaft x4 |
| `CCS_PrimitiveCampfireKitRecipe` | Hand | Branch x5 | Campfire Kit x1 |

Recipes are unlocked by default and registered through `CCS_StarterLoadoutService.RegisterPrimitiveRecipes()`. Bow assembly, arrow fletching, fire placement, and cooking are deferred.

---

## Service registration (0.5.3)

`CCS_SurvivalGameplayServiceRegistration` registers `CCS_CraftingService` after `CCS_PlayerInventoryService`:

| Host | Profile field |
|------|----------------|
| `CCS_SurvivalGameplayServiceHost` | `craftingProfile` → `CCS_DefaultCraftingProfile.asset` |

`CCS_CraftingRuntimeBridge` resolves crafting and inventory services from `CCS_RuntimeHost.ServiceRegistry` for harnesses and future station interactions.

---

## Crafting service flow

1. Receive `CCS_CraftingRequest` with recipe and station context.
2. Validate recipe definition and unlock state.
3. Validate station type against recipe and profile (`AllowHandCrafting` for Hand recipes).
4. Validate inventory ingredients via `HasItem`.
5. Validate output capacity via `CanAdd` **before** removing ingredients.
6. Remove ingredients with `RemoveItem` (partial rollback if consumption fails mid-recipe).
7. Grant results with `AddItem` (restore ingredients if grant fails).
8. Raise crafting requested/started/completed/failed events.

Failures return `CCS_CraftingResult.Failure` — no silent ingredient or output loss.

---

## HUD notification flow (0.5.3)

`CCS_HudPresentationService.BindCraftingService()` subscribes to:

| Event | Notification |
|-------|----------------|
| Crafting completed | `Crafting Completed: {RecipeName}` |
| Crafting failed | `Crafting Failed: {Reason}` |
| Recipe unlocked | `Recipe Unlocked: {RecipeName}` |

Successful crafts also refresh the inventory summary snapshot.

Wiring path: `CCS_HudGameplayServiceWiring` → `CCS_RuntimeHost.ServiceRegistry` → `CCS_CraftingService`.

---

## Development test harness

`CCS_CraftingTestHarness` (bootstrap scene only):

- Waits until inventory contains test recipe ingredients (from harvest loop).
- Attempts bandage recipe first, then campfire recipe.
- Rate-limited (`craftAttemptIntervalSeconds`, default 5s).
- Each test recipe is attempted at most once per session.
- Disable via Inspector `enableHarness` for shipping builds.

Not final player input. No infinite craft loop.

---

## Deferred systems

| System | Status |
|--------|--------|
| Crafting UI | Deferred |
| Final world station objects / interaction | Deferred |
| Timed craft queue processing | Data shape only |
| Save/load recipe unlock persistence | Deferred |
| Final player input actions | Deferred |

---

## Validation

**Editor menu:** **CCS → Survival → Crafting → Validate Crafting**

Validator ID: `ccs.survival.validation.crafting`

Checks include service registration, default profile, test recipes/items, ingredient/result validity, station types, and bootstrap prefab crafting profile assignment.

---

## Related docs

- [Inventory Module](../Inventory/Documentation/CCS_Inventory_Module.md)
- [UI HUD Module](../UI/Documentation/CCS_UI_HUD_Module.md)
- [World Resources Module](../WorldResources/Documentation/CCS_World_Resources_Module.md)
- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
