# CCS Fishing Module (1.2.5 Foundation, 1.2.6 Frontier Crafting)

## Summary

Service-driven frontier fishing foundation using `CCS_FishingService`, `CCS_FishingSpot` water interactions, `ActiveItemService` routing for the fishing pole, and inventory catch grants. **No minigame, casting animation, IK, line simulation, or final hotbar UI** in this milestone.

**Author:** James Schilz  
**Milestone:** 1.2.5

## Runtime safety (2.1.1)

- `CCS_FishingRuntimeBridge` resolves `CCS_FishingService` only when `CCS_RuntimeHost.ServiceRegistry` is non-null and the service is initialized (same pattern as `CCS_SleepRuntimeBridge`).
- `CCS_FishingSpot` defers registration in `OnEnable` / `Start` / `Update` until the service exists — no startup `NullReferenceException` when the bootstrap host is not ready.

## Placement

- Runtime: `Assets/CCS/Modules/Fishing/Runtime/`
- Profile: `Assets/CCS/Survival/Profiles/Fishing/CCS_DefaultFishingProfile.asset`
- Test prefab: `Assets/CCS/Survival/Prefabs/Fishing/PF_CCS_TestFishingSpot.prefab`
- Bootstrap scene object: `CCS_TestFishingSpot` under `CCS_FishingTestArea`

## Active item routing

When the active item is a **Tool** with `CCS_ItemToolType.FishingPole`:

1. `CCS_ActiveItemTargetResolver` resolves `CCS_FishingSpot` from the interaction current target.
2. `CCS_ActiveItemService` calls `CCS_FishingService.TryFish`.
3. Results map to `FishingSuccess`, `FishingFailed`, `FishingNoBait`, `FishingNoWater`, or `FishingTargetUnavailable`.

`HarvestMethodType.Fish` is **not** routed through generic gathering harvest; fishing uses the dedicated fishing path.

## Resource metadata

Fishing spots use:

- `CCS_ResourceSourceType.Water`
- `CCS_HarvestMethodType.Fish`
- Required tool: `CCS_ItemToolType.FishingPole`

## Items (primitive frontier)

| Item | Id |
|------|-----|
| Fishing Pole | `ccs.survival.item.tool.fishingpole` |
| Crude Hook | `ccs.survival.item.tool.crudehook` |
| Fishing Line | `ccs.survival.item.tool.fishingline` |
| Bait | `ccs.survival.item.consumable.bait` |
| Raw Fish | `ccs.survival.item.resource.rawfish` |
| Small Fish | `ccs.survival.item.resource.smallfish` |
| Junk | `ccs.survival.item.resource.junk` |

### Hand recipes (1.2.6 — `CCS_FrontierStarterProgressionBootstrapSetup`)

| Recipe ID | Output | Costs |
|-----------|--------|-------|
| `ccs.survival.recipe.frontier.fishingline` | Fishing Line | Fiber x2 |
| `ccs.survival.recipe.frontier.crudehook.bone` | Crude Hook | Bone x1 |
| `ccs.survival.recipe.frontier.crudehook.scrap` | Crude Hook | Scrap Iron x1 |
| `ccs.survival.recipe.frontier.fishingpole` | Fishing Pole | Sapling + Fishing Line + Crude Hook |

**Cooked Fish:** `ccs.survival.item.food.cookedfish` via campfire (`ccs.survival.cooking.recipe.cookfish`, 1.3.4). **Dried Fish:** smoke preservation recipe (`ccs.survival.cooking.recipe.smokedriedfish`).

## Economy (1.3.0)

Raw and small fish can be **sold** at the General Store for Trade Dollars (`ccs.survival.currency.tradedollars`). Purchase of fish at vendors is disabled in frontier bootstrap content.

## Bootstrap batch

```
CCS.Modules.Fishing.Editor.CCS_FishingBootstrapSetup.ExecuteBatch
```

## Future extensions

- Fishing minigame and tension mechanics
- Bait consumption policies per water body
- Salt curing for dried fish (smoke-only in 1.3.4)
- Multiplayer authority on `CCS_FishingRequest` / spot state
