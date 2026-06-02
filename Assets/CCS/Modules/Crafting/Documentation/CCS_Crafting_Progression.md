# CCS Crafting Progression — Workstation Foundation

**Milestone:** 1.2.6 — Frontier starter hand recipes + 1.1.1 workstation foundation  
**Author:** James Schilz (Developer)  
**Date:** 2026-06-01

## Purpose

Milestone **1.1.1** organizes primitive survival crafting into a clear progression path using existing inventory and crafting systems. Players gather resources, craft tools by hand, build shelter, then use campfire and workbench stations for stronger items.

**1.2.6** adds Western frontier hand recipes under `Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes/` (camp, cordage, fishing pole, bow/arrows, trap). Spear hand recipe is legacy/regression — not default starter progression.

Design direction: CCS Survival progression is shifting toward realistic Western frontier survival: knife, camp, water, bow, fishing, traps, salvage, shelter, tools, and trade.

No new art packs, item database editor, or multiplayer in this milestone.

## Station contexts

| Player label | `CCS_CraftingStationType` | Usage |
|--------------|---------------------------|--------|
| Hand | `Hand` | Inventory crafting anywhere |
| Campfire | `FirePit` | Interact with campfire (`CCS_CraftingStationInteractable`) |
| Workbench | `Workbench` | Interact with `CCS_TestWorkbench` |

## Gameplay flow

1. Gather wood, stick, stone, plant fiber from world nodes  
2. Hand-craft spear, bandage, torch  
3. Build primitive shelter (milestone 1.1.0)  
4. Interact with campfire for charcoal, ash, dried meat  
5. Interact with workbench for reinforced spear, storage crate, bedroll  

## Recipe list

### Hand — frontier starter (1.2.6)

| Recipe ID | Output | Costs (summary) |
|-----------|--------|-----------------|
| `ccs.survival.recipe.frontier.fishingpole` | Fishing Pole | Sapling + Fishing Line + Crude Hook |
| `ccs.survival.recipe.frontier.bow` | Bow | Sapling + Cordage |
| `ccs.survival.recipe.frontier.arrow` | Arrow | Stick + Flint + Feather |
| `ccs.survival.recipe.frontier.cordage` | Cordage | Fiber |
| `ccs.survival.recipe.frontier.fishingline` | Fishing Line | Fiber |
| `ccs.survival.recipe.frontier.simpletrap` | Simple Trap | Sapling + Cordage |
| `ccs.survival.recipe.frontier.campfire` | Campfire Kit | Wood + Stone + Tinder Bundle |

See `Assets/CCS/Survival/Documentation/CCS_Starter_Loadout.md` for the full frontier recipe list.

### Hand — legacy progression (workbench path)

| Recipe ID | Output | Costs |
|-----------|--------|-------|
| `ccs.survival.recipe.progression.spear` | Spear | Stick x2, Stone x1 |
| `ccs.survival.recipe.progression.basicbandage` | Basic Bandage | Plant Fiber x2 |
| `ccs.survival.recipe.progression.primitivetorch` | Primitive Torch | Stick x1, Plant Fiber x1 |

### Campfire (FirePit)

| Recipe ID | Output | Costs |
|-----------|--------|-------|
| `ccs.survival.recipe.progression.charcoal` | Charcoal | Wood x1 |
| `ccs.survival.recipe.progression.ash` | Ash | Wood x1 |
| `ccs.survival.recipe.progression.driedmeat.rabbit` | Dried Meat | Cooked Rabbit Meat x1 |
| `ccs.survival.recipe.progression.driedmeat.venison` | Dried Meat | Cooked Venison x1 |

### Workbench

| Recipe ID | Output | Costs |
|-----------|--------|-------|
| `ccs.survival.recipe.progression.reinforcedspear` | Reinforced Spear | Spear x1, Wood x2, Stone x2 |
| `ccs.survival.recipe.progression.storagecrate` | Storage Crate | Wood x6 |
| `ccs.survival.recipe.progression.bedroll` | Bedroll | Plant Fiber x4, Stick x2 |

## New item IDs

| Item ID | Notes |
|---------|--------|
| `ccs.survival.item.progression.basicbandage` | Consumable placeholder |
| `ccs.survival.item.progression.primitivetorch` | Tool placeholder |
| `ccs.survival.item.progression.charcoal` | Material |
| `ccs.survival.item.progression.ash` | Material |
| `ccs.survival.item.progression.driedmeat` | Consumable |
| `ccs.survival.item.progression.reinforcedspear` | Weapon (higher damage than spear) |
| `ccs.survival.item.progression.storagecrate` | Placeable building metadata in description |
| `ccs.survival.item.starter.bedroll` | Existing bedroll (sleep metadata in description) |

## Runtime types

| Type | Role |
|------|------|
| `CCS_CraftingProgressionProfile` | Recipe catalog, debug logging, workbench playtest recipe id |
| `CCS_CraftingProgressionRecipeEntry` | Recipe asset + unlock tier |
| `CCS_CraftingRecipeService` | Lookup, station filter, orchestration, progression events |
| `CCS_CraftingStationInteractable` | Sets active station on interact |

## Events

| Event | When |
|-------|------|
| `CraftingRecipeValidated` | Recipe and station authorized |
| `CraftingRecipeFailed` | Validation or craft failure |
| `CraftingStarted` | Craft attempt begins |
| `CraftingResourcesConsumed` | Inputs removed successfully |
| `CraftingCompleted` | Outputs granted |

Legacy `CCS_CraftingService` events still fire through the underlying craft call.

## Workbench setup

- Scene object: `CCS_TestWorkbench` (primitive cube) in bootstrap crafting test area  
- Component: `CCS_CraftingStationInteractable` (`Workbench`, id `ccs.survival.station.test.workbench`)  
- Campfire: `CCS_TestCampfire` also receives `CCS_CraftingStationInteractable` (`FirePit`)

## Playtest integration

After **Build shelter**, checklist step **Craft item at workbench** (`CraftAtWorkbench`):

- **F4** — seed workbench crafting materials (dev)  
- **F3** — craft storage crate at workbench context (dev; avoids F8 save-debug conflict)  
- Auto-completes when any workbench recipe finishes via `CCS_CraftingRecipeService.CraftingCompleted`

## Bootstrap batch

```text
CCS.Modules.Crafting.Editor.CCS_CraftingProgressionBootstrapSetup.ExecuteBatch
CCS.Modules.Playtesting.Editor.CCS_PlaytestBootstrapSetup.ExecuteBatch
```

## Related docs

- [CCS_Crafting_Module.md](CCS_Crafting_Module.md)
- [CCS_Playtesting_Module.md](../../Playtesting/Documentation/CCS_Playtesting_Module.md)
