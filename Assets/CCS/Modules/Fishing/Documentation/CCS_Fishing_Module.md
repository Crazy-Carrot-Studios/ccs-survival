# CCS Fishing Module (1.2.5 Foundation)

## Summary

Service-driven frontier fishing foundation using `CCS_FishingService`, `CCS_FishingSpot` water interactions, `ActiveItemService` routing for the fishing pole, and inventory catch grants. **No minigame, casting animation, IK, line simulation, or final hotbar UI** in this milestone.

**Author:** James Schilz  
**Milestone:** 1.2.5

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

### Recipe intent (crafting wiring deferred)

- **Fishing Pole:** Sapling x1 + Plant Fiber or Rawhide Cord x1 + Crude Hook x1
- **Crude Hook:** Bone x1 **or** Scrap Iron x1
- **Line:** Plant Fiber x2 **or** Rawhide Cord x1
- **Cooked Fish:** reserved for Cooking module integration

## Bootstrap batch

```
CCS.Modules.Fishing.Editor.CCS_FishingBootstrapSetup.ExecuteBatch
```

## Future extensions

- Fishing minigame and tension mechanics
- Bait consumption policies per water body
- Cooking integration for cooked fish
- Multiplayer authority on `CCS_FishingRequest` / spot state
