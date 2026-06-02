# CCS Cooking Module

**Milestone:** 1.3.4 — Cooking + Food Preservation Expansion  
**Author:** James Schilz  
**Date:** 2026-06-01

## Purpose

Generic campfire cooking and consumable food framework. Frontier fish, meat, and smoke-preserved trail food live in Survival content assets and `CCS_DefaultCookingProfile`.

## Food Loop (1.3.4)

```text
Fish / Hunt / Trap
  ↓
Raw Food
  ↓
Cook or Preserve (campfire)
  ↓
Eat or Sell
  ↓
Survive / Earn Dollars
```

## Scope

| Included | Excluded |
|----------|----------|
| Campfire recipes (cook + smoke preserve) | Final cooking UI |
| `CCS_ConsumableFoodService` hunger restore | Recipe queue UI |
| Frontier fish/meat/jerky/dried fish items | Final food art |
| General Store buy/sell for trail food | Salt curing (smoke-only placeholder) |
| Playtest cooking checklist | Advanced spoilage |

## Preservation (smoke-only)

Salt is not implemented. Preservation uses longer campfire recipes:

| Recipe ID | Raw | Output | Fuel | Duration |
|-----------|-----|--------|------|----------|
| `ccs.survival.cooking.recipe.smokejerky` | Raw Meat x1 | Jerky x1 | Stick/Wood x2 | 12s |
| `ccs.survival.cooking.recipe.smokedriedfish` | Raw Fish x1 | Dried Fish x1 | Stick/Wood x2 | 10s |

## Cook recipes (1.3.4)

| Recipe ID | Raw | Cooked |
|-----------|-----|--------|
| `ccs.survival.cooking.recipe.cookfish` | Raw Fish | Cooked Fish |
| `ccs.survival.cooking.recipe.cooksmallfish` | Small Fish | Cooked Fish |
| `ccs.survival.cooking.recipe.cookmeat` | Raw Meat | Cooked Meat |
| `ccs.survival.cooking.recipe.cookrabbit` | Raw Rabbit Meat | Cooked Rabbit |
| `ccs.survival.cooking.recipe.cookvenison` | Raw Venison | Cooked Venison |
| `ccs.survival.cooking.recipe.cookturkey` | Raw Turkey Meat | Cooked Turkey |

## Hunger restore (selected)

| Food | Hunger |
|------|--------|
| Cooked Venison | 50 |
| Cooked Turkey | 45 |
| Cooked Meat | 40 |
| Cooked Rabbit | 35 |
| Cooked Fish | 28 |
| Jerky | 18 |
| Dried Fish | 15 |
| Hardtack | 12 |
| Raw Venison | 12 |
| Raw Turkey | 10 |
| Raw Rabbit | 8 |
| Raw Meat | 6 |
| Raw Fish | 5 |

Cooked meals always restore more hunger than their raw counterparts.

## Bootstrap / validation

- `CCS_FrontierCookingBootstrapSetup.ExecuteBatch` — items, profile, vendor, playtest (1.3.4)
- `ccs.survival.validation.cooking.frontier` — registered on survival validation pipeline

## Deferred

- Salt ingredient and salted meat recipes
- Cooking station selection UI
- Fuel burn-down simulation
- Spoilage and food poisoning
