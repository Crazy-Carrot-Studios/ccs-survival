# CCS Ranching Module

Milestone **2.1.0** — Ranching Foundation

**Author:** James Schilz  
**Date:** 2026-06-02

## Purpose

Generic livestock and ranch structure framework for timer-based production, homestead association, economy integration, and world simulation food supply. Western frontier content (chicken, goat, cow, pig, eggs, milk) lives in Survival assets under `Assets/CCS/Survival/Content/Ranching/`.

No advanced animal AI, breeding, farming, final art, or final ranch UI in this milestone.

## Ranching Loop

```text
Buy Livestock
      ↓
Place Ranch Structure
      ↓
Assign Livestock (coop / pen)
      ↓
Produce Eggs / Milk
      ↓
Collect Goods
      ↓
Sell Goods
      ↓
Increase Settlement Food Supply
```

## Architecture

| Component | Role |
|-----------|------|
| `CCS_LivestockProfile` | Catalog of livestock and ranch structure definitions |
| `CCS_LivestockDefinition` | Production interval, feed/water requirements, output item |
| `CCS_RanchStructureDefinition` | Placeable coop, pen, feed trough, water trough kits |
| `CCS_RanchService` | Ownership, placement, assignment, timer production, collection |
| `CCS_RanchRuntimeBridge` | Resolves service from `CCS_RuntimeHost` registry |
| `CCS_RanchValidationUtility` | Profile and content validation |

## Livestock States

- Idle
- Assigned
- Producing
- ReadyToCollect
- Unavailable

## Production Rules

- Livestock must be assigned to a valid structure (coop for chickens, pen for goats/cows/pigs).
- Feed and water troughs must be within configured radius when `requiresFeed` / `requiresWater` are set.
- Collection adds items to player inventory; fails safely when inventory is full.
- Output is profile-driven via `productionItem` and `productionQuantity`.

## Integration

| System | Hook |
|--------|------|
| Economy | General Store / Stable sell livestock, feed, ranch kits; General Store buys eggs and milk |
| World Simulation | Egg and milk vendor sells map to settlement Food supply |
| Camp | Ranch structures with `contributesToCampTier` register Livestock camp presence |
| Save | `CCS_SaveRanchingWorldData` persists livestock and structure snapshots |
| Land (2.3.0) | Ranch structures inside claim radius associate via `CCS_LandClaimService` hooks |
| Active Item | `BindFrontierRanchPlacementHandler` routes ranch kit placement without Hotbar→Ranching asmdef dependency |
| Playtest | Steps 135–143 cover buy → place → assign → produce → collect → sell → food supply → save/load |

## Bootstrap

Run `CCS_RanchingFoundationBootstrapSetup.ExecuteBatch()` to generate profile, content, vendor catalog entries, playtest steps, and bootstrap host wiring.

## Future

- `CCS_CampTier.LivestockRanch` placeholder tier (not required for 2.1.0)
- **Feed placeholder (2.2.0):** corn and wheat crop definitions are tagged `isFutureLivestockFeed` for future ranch feed consumption (not implemented in 2.2.0)
- Breeding, roaming AI, meat processing, final art and UI
