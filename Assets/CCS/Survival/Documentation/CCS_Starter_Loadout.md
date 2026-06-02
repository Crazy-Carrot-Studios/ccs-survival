# CCS Starter Loadout — Western Frontier (1.3.1)

**Author:** James Schilz  
**Date:** 2026-06-01  
**Profile:** `Assets/CCS/Survival/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset`  
**Bootstrap:** `CCS.Survival.Editor.Development.CCS_FrontierStarterProgressionBootstrapSetup.ExecuteBatch`

## Design direction

CCS Survival progression is shifting toward realistic Western frontier survival: knife, camp, water, bow, fishing, traps, salvage, shelter, tools, and trade.

## Default starter supplies (1.2.6)

| Item ID | Display | Qty | Role |
|---------|---------|-----|------|
| `ccs.survival.item.starter.knife` | Pocket Knife | 1 | Primary tool/weapon (active item + harvest) |
| `ccs.survival.item.starter.bedroll` | Bedroll | 1 | Sleep / shelter placeholder |
| `ccs.survival.item.starter.canteen` | Canteen | 1 | Water container placeholder |
| `ccs.survival.item.starter.hardtack` | Hardtack Ration | 3 | Food ration placeholder |
| `ccs.survival.item.starter.dollars` | Trade Dollars | — | Currency (10 starting via profile) |
| `ccs.survival.item.starter.tinderbox` | Tinderbox | 1 | Matches/tinder placeholder |

**Spear is not in the default loadout.** `ccs.survival.item.starter.spear` remains for regression/playtest only.

## Hand crafting (frontier primitive recipes)

Registered on the starter profile as `primitiveRecipes` (hand station):

| Recipe ID | Output |
|-----------|--------|
| `ccs.survival.recipe.frontier.tinderbundle` | Tinder Bundle |
| `ccs.survival.recipe.frontier.cordage` | Cordage |
| `ccs.survival.recipe.frontier.fishingline` | Fishing Line |
| `ccs.survival.recipe.frontier.crudehook.bone` / `.scrap` | Crude Hook |
| `ccs.survival.recipe.frontier.fishingpole` | Fishing Pole |
| `ccs.survival.recipe.frontier.bow` | Bow (no ranged active behavior yet) |
| `ccs.survival.recipe.frontier.arrow` | Arrow |
| `ccs.survival.recipe.frontier.simpletrap` | Simple Trap |
| `ccs.survival.recipe.frontier.campfire` | Campfire Kit |
| `ccs.survival.recipe.frontier.torch` | Primitive Torch (when progression torch asset exists) |

Ingredients use practical sources from **1.2.4** (sapling, fiber, stick, wood, stone, bone, scrap iron, flint, fat) — not random terrain rock/stick nodes as primary progression.

## Runtime

`CCS_StarterLoadoutService` applies the profile when inventory is empty on first play. `CCS_SaveStartupLoader` skips duplicate grants after a save exists.

## Economy (1.3.1)

Starter Trade Dollars sync to `CCS_CurrencyService` after loadout via inventory backing. Fish and salvage can be sold at the frontier General Store.

**Bone hatchet is not starter gear.** Purchase it from the General Store after earning Trade Dollars (see economy module). This keeps the knife-only traveler identity while proving trade → tool → wood harvest progression.
