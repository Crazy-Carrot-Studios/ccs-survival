# CCS Cooking Module

**Milestone:** 0.9.4 — Campfire & Cooking Foundation

## Purpose

Completes the first self-sustaining survival loop:

Knife → Branches → Campfire Kit → Campfire → Raw Meat → Cooked Meat → Hunger Restoration

## Scope (0.9.4)

| Included | Excluded |
|---|---|
| Campfire placement via Building framework | Combat |
| Campfire interactable (Unlit / Lit / Cooking) | Cooking UI |
| Raw Meat → Cooked Meat on lit campfire | Fuel systems |
| Basic Food + Cooked Meat hunger restore | Health restore |
| HUD notifications | Crafting stations beyond campfire |

## Key Types

| Type | Role |
|---|---|
| `CCS_CookingProfile` | Cooking, campfire, and consumable food tuning |
| `CCS_CampfireDefinition` | Campfire identity and cook timing |
| `CCS_CampfireService` | Campfire state, kit placement, lit/cook orchestration |
| `CCS_CookingService` | Ingredient validation, cook queue, output grant |
| `CCS_ConsumableFoodService` | Inventory food consumption and hunger restore |
| `CCS_CampfireInteractable` | Light fire and cook meat interactions |

## Player Controls (Bootstrap)

| Input | Action |
|---|---|
| Interact | Light campfire / cook meat on targeted campfire |
| B | Toggle campfire kit placement mode |
| Interact (while placing) | Confirm campfire placement |
| F | Consume first available configured food item |

## Validation

Menu: **CCS → Survival → Cooking → Validate Cooking**

Registered on `CCS_SurvivalValidationPipeline` via `CCS_CookingValidationRegistration`.

## Deferred

- Fuel and BurnedOut gameplay
- Cooking UI and recipe browser
- FirePit crafting station world objects beyond campfire interactable
- Stamina bonus from cooked food
