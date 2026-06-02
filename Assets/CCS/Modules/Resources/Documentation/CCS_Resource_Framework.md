# CCS Resource Framework

**Milestone:** 1.7.0 — Prospecting and mining expansion (pick tiers, iron ore veins, test nodes)  
**Previous:** 1.2.4 — Frontier Resource Framework Audit  
**Module ID:** `ccs.survival.resources`  
**Namespace:** `CCS.Modules.Resources`  
**Author:** James Schilz  
**Date:** 2026-06-01  

---

## Design rule

**CCS Survival uses practical resource sources** such as trees, outcrops, deadfall, fiber plants, water sources, ore veins, and salvage sites **rather than cluttering terrain with random pickup rocks and sticks.**

Legacy bootstrap nodes (`SmallTree`, `Rock`, `Bush`) remain for regression tests. Frontier progression uses named source archetypes and multi-drop yields.

---

## Classifications

### `CCS_ResourceSourceType`

| Value | Use |
|-------|-----|
| Natural | Trees, outcrops, fiber, water |
| Wildlife | Carcass skin/butcher via Wildlife module (1.3.2); trap harvest reuses same harvest tables (1.3.3) |
| Salvage | Wagon, camp, ruins, mine debris |
| Mining | Ore and coal veins |
| Water | Collectable water sources |
| Agriculture | Crops (future) |
| Crafted | Processed stations |
| Other | Extension |

### `CCS_HarvestMethodType`

| Value | Default tool | Active use (1.2.4) |
|-------|--------------|---------------------|
| Gather / Collect | None | Supported |
| Chop | Axe | Supported |
| Mine | Pickaxe | Supported |
| Dig | Shovel | Supported |
| Salvage | None | Supported |
| Skin / Butcher | Knife | Wildlife harvest (`CCS_WildlifeHarvestDefinition`, 1.3.2); triggered traps via `CCS_TrapService` + `CCS_WildlifeHarvestService` (1.3.3) |
| Fish | FishingPole | **Fishing module (1.2.5)** via `CCS_FishingService` (not gathering harvest) |

Explicit `requiredToolType` on definitions overrides defaults.

---

## Yield model

| System | Multi-drop support |
|--------|-------------------|
| **Gathering** | `CCS_GatheringNodeRewardSettings.rewards[]` — multiple `CCS_GatheringReward` per node type |
| **WorldResources** | `CCS_ResourceDefinition.dropDefinitions` — multiple `CCS_ResourceDropDefinition` with min/max quantities |

No schema changes were required for 1.2.4; both systems already support multiple drops per source.

---

## Integration

- **Gathering:** `CCS_GatheringNodeRewardSettings` carries `resourceSourceType`, `harvestMethod`, `requiredToolType`.
- **WorldResources:** `CCS_ResourceDefinition` carries `resourceSourceType`, `harvestMethod`.
- **Active item (1.2.5):** `Fish` routes through `CCS_FishingService` when the active tool is a fishing pole; generic gathering harvest excludes `Fish`.

---

## Bootstrap

```text
CCS.Modules.Resources.Editor.CCS_FrontierResourceFrameworkBootstrapSetup.ExecuteBatch
```

Creates frontier item definitions, gathering profile entries, and world resource definition assets under `Assets/CCS/Survival/Content/Items/Resources/Frontier/` and `Assets/CCS/Survival/Profiles/WorldResources/Frontier/`.

---

## Deferred

- Terrain spawning and biome placement
- Fishing minigame, casting animation, line simulation (foundation in `CCS.Modules.Fishing` — see `CCS_Fishing_Module.md`)
- Western-specific hard-coded logic in generic systems
