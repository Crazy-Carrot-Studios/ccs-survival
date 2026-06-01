# CCS Building Progression — Tier 1 Primitive Shelter

**Milestone:** 1.1.0 — Building Progression Foundation  
**Author:** James Schilz (Developer)  
**Date:** 2026-06-01

## Purpose

Milestone **1.1.0** converts the building test harness into the first survival building progression loop: primitive tier-1 pieces, recipe costs, placement rules, inventory consumption, save integration, and playtest shelter completion.

Primitives only — no art packs, multiplayer, weather, or item database expansion in this milestone.

## Tier 1 pieces

| Category | Piece ID | Display name |
|----------|----------|--------------|
| Foundation | `ccs.survival.building.primitive.foundation` | Primitive Foundation |
| Wall | `ccs.survival.building.primitive.wall` | Primitive Wall |
| Doorway | `ccs.survival.building.primitive.doorway` | Primitive Doorway Wall |
| Floor | `ccs.survival.building.primitive.floor` | Primitive Floor |
| Roof | `ccs.survival.building.primitive.roof` | Primitive Roof |

Content assets live under `Assets/CCS/Survival/Content/Building/Primitive/`.

## Recipe costs

| Piece | Wood | Stick |
|-------|------|-------|
| Foundation | 4 | 2 |
| Wall | 3 | — |
| Doorway | 4 | — |
| Floor | 2 | — |
| Roof | 4 | 2 |

Costs are defined on `CCS_BuildingProgressionProfile` recipes and enforced by `CCS_BuildingRecipeService` when `progressionEnabled` is true.

## Placement rules

| Piece | Rules |
|-------|--------|
| Foundation | Free placement (no snap required) |
| Wall | Foundation nearby (12 m search) + snap |
| Doorway | Foundation nearby + snap |
| Floor | Foundation nearby + snap |
| Roof | Snap to wall or doorway support |

Validation is implemented in `CCS_BuildingProgressionPlacementUtility` and invoked from `CCS_BuildingRecipeService.TryAuthorizePlacement`.

## Runtime types

| Type | Role |
|------|------|
| `CCS_BuildingPieceCategory` | Foundation, Wall, Doorway, Floor, Roof |
| `CCS_BuildingRecipe` | Recipe id, display name, category, costs, placement rules |
| `CCS_BuildingProgressionProfile` | Enabled pieces, recipes, shelter minimums, debug logging |
| `CCS_BuildingRecipeService` | Lookup, cost validation, placement authorization, shelter counting |
| `CCS_BuildingPieceDefinition` | Category, prefab reference, legacy/build costs |

## Placement flow

When the player places a piece through `CCS_BuildingPlacementService`:

1. Resolve recipe for the target piece id.
2. `TryAuthorizePlacement` — placement rules + inventory preview.
3. `TryConsumeRecipeCosts` — remove wood/stick (or legacy definition costs if progression disabled).
4. Spawn piece and register with `CCS_BuildingService`.
5. Persist through the existing building saveable.
6. Raise progression events (`BuildingRecipeValidated`, `BuildingResourcesConsumed`, `BuildingPiecePlaced`) and placement events.

## Events

| Event | When |
|-------|------|
| `BuildingRecipeValidated` | Recipe and placement rules passed |
| `BuildingRecipeFailed` | Missing recipe, failed rules, or insufficient inventory |
| `BuildingResourcesConsumed` | Costs removed after successful authorization |
| `BuildingPiecePlaced` | Piece spawned and registered |

## Shelter minimum (playtest)

`CCS_BuildingProgressionProfile` defines shelter completion thresholds (default **1** foundation, **1** wall, **1** roof).

`CCS_PlaytestService` completes the **Build shelter** checklist step when `CCS_BuildingRecipeService.MeetsMinimumShelter()` returns true after any building placement event.

Manual playtest hotkey **B** still seeds costs and places a primitive foundation for quick iteration.

## Profiles and bootstrap

| Asset | Path |
|-------|------|
| Progression profile | `Assets/CCS/Survival/Profiles/Building/CCS_DefaultBuildingProgressionProfile.asset` |
| Building catalog | `Assets/CCS/Survival/Profiles/Building/CCS_DefaultBuildingProfile.asset` |

Editor bootstrap (batch):

```text
CCS.Modules.Building.Editor.CCS_BuildingProgressionBootstrapSetup.ExecuteBatch
```

Playtest profile refresh (batch):

```text
CCS.Modules.Playtesting.Editor.CCS_PlaytestBootstrapSetup.ExecuteBatch
```

## Validation

Building validator checks progression profile, five primitive definitions, recipe costs, placement rules, composition wiring (`CreateBuildingRecipeService`, `BindRecipeService`), and **BuildShelter** playtest step.

Run full pipeline:

```text
CCS.Survival.Editor.Development.CCS_SurvivalValidationMenu.RunSurvivalValidation
```

## Related docs

- [CCS_Building_Module.md](CCS_Building_Module.md) — module overview (placement, snap, persistence)
- [CCS_Playtesting_Module.md](../../Playtesting/Documentation/CCS_Playtesting_Module.md) — manual checklist harness
