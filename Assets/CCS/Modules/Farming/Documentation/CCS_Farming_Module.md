# CCS Farming Module

**Milestone:** 2.2.0 — Farming Foundation  
**Author:** James Schilz (Developer)  
**Date:** 2026-06-02

---

## Purpose

Generic crop and farm-plot framework for frontier agriculture: place plots, plant seeds, timer-based growth, harvest food, sell to vendors, and contribute settlement **Food** supply through World Simulation.

Not included in 2.2.0: full terrain farming, seasons, irrigation, fertilizer, pests, final crop art, or farming UI.

---

## Farming loop

```text
Buy Seeds
    ↓
Place Farm Plot
    ↓
Plant Crop
    ↓
Grow (timer stages)
    ↓
Harvest Food
    ↓
Sell / Supply Settlement
```

---

## Runtime types

| Type | Role |
|------|------|
| `CCS_CropProfile` | Catalog of `CCS_CropDefinition` + `CCS_FarmPlotDefinition` (host profile) |
| `CCS_CropDefinition` | Seed item, harvest item, growth duration, feed placeholder flags |
| `CCS_FarmPlotDefinition` | Placeable kit item, placement rules, one crop per plot |
| `CCS_FarmService` | Plot placement, planting, growth tick, harvest, save/restore |
| `CCS_FarmPlotInstance` / `CCS_CropInstance` | Runtime state |
| `CCS_CropGrowthStage` | Empty → Planted → Sprouting → Growing → Mature → Harvested |
| `CCS_FarmPlotInteractable` | Interact (E) to harvest mature crops |
| `CCS_FarmRuntimeBridge` | Null-safe service resolution |

---

## Content (Survival assets)

| Item | Path / ID |
|------|-----------|
| Profile | `Assets/CCS/Survival/Profiles/Farming/CCS_DefaultCropProfile.asset` |
| Crops | Corn, Beans, Potatoes, Wheat under `Assets/CCS/Survival/Content/Farming/Crops/` |
| Prefabs | `Assets/CCS/Survival/Prefabs/Farming/PF_CCS_Crop_*` (primitive placeholders) |
| Farm plot kit | `ccs.survival.farming.item.farmplotkit` |

Corn and wheat are tagged **`isFutureLivestockFeed`** for future ranch integration (no feed consumption in 2.2.0).

---

## Integration

- **Composition:** `CCS_SurvivalGameplayServiceHost.farmingProfile` → `CreateFarmService`
- **Active item:** farm plot kit (preview/confirm placement); seed items (plant in nearest empty plot)
- **Economy:** General Store sells kit + seeds; buys harvest crops
- **World Simulation:** harvest item IDs route to **Food** supply (same path as ranch eggs/milk)
- **Save:** `CCS_SaveData.farming.plots[]` via `CCS_FarmPlotSnapshot` + nested `CCS_CropSnapshot`
- **Playtest:** Farming HUD group; Ctrl+Shift+P shortcut; 10 playtest steps

---

## Bootstrap

Run **`CCS_FarmingFoundationBootstrapSetup.ExecuteBatch`** (Unity batch or menu) to create content, vendor rows, profile assignment, and playtest steps.

---

## Future extension hooks

- Seasons, soil quality, water requirement (profile-driven)
- Irrigation and fertilizer
- Larger fields / multi-tile plots
- **Farmstead** homestead tier (placeholder only in 2.2.0)
- Livestock feed consumption for corn/wheat
