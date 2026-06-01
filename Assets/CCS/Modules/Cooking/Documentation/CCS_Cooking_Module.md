# CCS Cooking Module

**Milestone:** 1.0.0 — Campfire + Cooking Foundation

## Purpose

Completes the primitive survival loop: gather fuel, hunt wildlife, harvest meat, cook on a campfire, and eat cooked food for stronger hunger restoration.

## Scope (1.0.0)

| Included | Excluded |
|---|---|
| `CCS_CookingStation` campfire stations | Advanced cooking UI |
| Fuel-backed recipes (stick or wood) | Fuel burn simulation over time |
| Rabbit and venison cook recipes | Multi-station queue UI |
| `CCS_CookingInteractable` auto-starts first valid recipe | Multiplayer replication |
| Species-specific raw and cooked meat items | Custom campfire art |

## Gather → Hunt → Cook → Eat Loop

1. Gather **stick** or **wood** from `CCS_GatheringTestArea` nodes.
2. Hunt rabbit or deer with equipped spear (`CCS_CombatService`).
3. Harvest carcass for species-specific raw meat.
4. Interact with `CCS_TestCampfire` (`CCS_CookingInteractable`).
5. Service consumes raw meat + fuel, cooks over time, grants cooked meat.
6. Press **F** to consume cooked food (`CCS_ConsumableFoodService`).

## Item IDs (1.0.0)

| Item | Item ID |
|---|---|
| Raw Rabbit Meat | `ccs.survival.item.resource.rawrabbitmeat` |
| Raw Venison | `ccs.survival.item.resource.rawvenison` |
| Cooked Rabbit Meat | `ccs.survival.item.food.cookedrabbitmeat` |
| Cooked Venison | `ccs.survival.item.food.cookedvenison` |
| Stick (fuel) | `ccs.survival.item.resource.stick` |
| Wood (fuel) | `ccs.survival.item.resource.wood` |
| Legacy Raw Meat | `ccs.survival.item.resource.rawmeat` |
| Legacy Cooked Meat | `ccs.survival.item.food.cookedmeat` |

## Default Recipes

| Recipe ID | Raw | Cooked | Fuel | Duration |
|---|---|---|---|---|
| `ccs.survival.cooking.recipe.cookrabbit` | Raw Rabbit x1 | Cooked Rabbit x1 | Stick or Wood x1 | 5s |
| `ccs.survival.cooking.recipe.cookvenison` | Raw Venison x1 | Cooked Venison x1 | Stick or Wood x1 | 7s |

## Hunger Restore (consumables)

| Food | Hunger restore |
|---|---|
| Cooked Rabbit Meat | 35 |
| Cooked Venison | 50 |
| Raw Rabbit Meat | 8 |
| Raw Venison | 12 |

## Validation

Menu: **CCS → Survival → Cooking → Validate Cooking**

Batch: `CCS.Modules.Cooking.Editor.CCS_CookingValidationMenu.ValidateCooking`

Bootstrap batch: `CCS.Modules.Cooking.Editor.CCS_CookingBootstrapSetup.ExecuteBatch`

## Deferred

- Cooking station selection UI
- Fuel burn-down while lit
- Recipe discovery and quality tiers
- Spoilage and food poisoning
