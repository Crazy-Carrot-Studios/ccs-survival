# CCS Cooking Module

**Milestone:** 0.9.5 — Consumables & Hunger Usage

## Purpose

Completes the first self-sustaining survival loop and makes consumable food useful during gameplay:

Knife → Branches → Campfire Kit → Campfire → Raw Meat → Cooked Meat → Hunger Restoration

## Scope (0.9.5)

| Included | Excluded |
|---|---|
| Campfire placement via Building framework | Combat |
| Campfire interactable (Unlit / Lit / Cooking) | Cooking UI |
| Raw Meat → Cooked Meat on lit campfire | Fuel systems |
| Basic Food (+15) + Cooked Meat (+40) hunger restore | Health restore |
| Consume cooldown and fullness checks | Buffs |
| Prefer Cooked Meat before Basic Food | Starvation health damage |
| HUD notifications for consume success/failure | Disease / spoilage |
| F key consume (temporary developer binding) | Full inventory UI |

## Key Types

| Type | Role |
|---|---|
| `CCS_CookingProfile` | Cooking, campfire, and consumable food tuning |
| `CCS_CampfireDefinition` | Campfire identity and cook timing |
| `CCS_CampfireService` | Campfire state, kit placement, lit/cook orchestration |
| `CCS_CookingService` | Ingredient validation, cook queue, output grant |
| `CCS_ConsumableFoodService` | Inventory food consumption, cooldown, hunger restore |
| `CCS_ConsumableFoodDefinition` | Hunger restore, optional cooldown override, notification label |
| `CCS_CampfireInteractable` | Light fire and cook meat interactions |

## Food Consumption Flow

1. Player presses **F** once (`Consume` gameplay action).
2. `CCS_ConsumableFoodPlayerDriver` calls `TryConsumeFirstAvailableFood()`.
3. Service validates survival core availability, cooldown, and hunger fullness.
4. Service prefers **Cooked Meat** (+40) over **Basic Food** (+15) when both exist.
5. On success: remove one item, apply hunger modifier, raise `FoodConsumed`.
6. On failure: raise `FoodConsumeFailed` with reason (no food, hunger full, cooldown, etc.).

Inventory food counts persist through existing inventory save paths when enabled. Consume cooldown is runtime-only and does not persist.

## Player Controls (Bootstrap)

| Input | Action |
|---|---|
| Interact | Light campfire / cook meat on targeted campfire |
| B | Toggle campfire kit placement mode |
| Interact (while placing) | Confirm campfire placement |
| F (Keyboard) / D-Pad Down (Gamepad) | Consume first available configured food item |

## Validation

Menu: **CCS → Survival → Cooking → Validate Cooking**

Registered on `CCS_SurvivalValidationPipeline` via `CCS_CookingValidationRegistration`.

## Deferred

- Fuel and BurnedOut gameplay
- Cooking UI and recipe browser
- FirePit crafting station world objects beyond campfire interactable
- Stamina bonus from cooked food
- Starvation health damage and death
