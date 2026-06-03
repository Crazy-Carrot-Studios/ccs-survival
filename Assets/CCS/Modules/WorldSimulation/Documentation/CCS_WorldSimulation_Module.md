# CCS World Simulation Module

Milestone **2.2.0** — Farming harvest goods (corn, beans, potatoes, wheat) route to settlement **Food** supply on vendor sell.

Milestone **2.1.0** — Ranching goods (egg, milk, future meat placeholders) route to settlement **Food** supply on vendor sell.

Milestone **2.0.0** — Frontier World Simulation Foundation

## Purpose

Provides a framework-level simulation layer for frontier settlements and regions. Tracks supply, demand, production, prosperity, and region resource potential metadata without NPC AI, quests, factions, procedural generation, or final UI.

## Architecture

| Component | Role |
|-----------|------|
| `CCS_WorldSimulationProfile` | Profile catalog for settlement defaults, region metadata, and vendor routes |
| `CCS_WorldSimulationService` | Runtime state, vendor supply integration, prosperity calculation, save/restore |
| `CCS_WorldSimulationSnapshot` | Aggregate query snapshot for settlements and regions |
| `CCS_WorldSimulationRuntimeBridge` | Resolves service from `CCS_RuntimeHost` registry |
| `CCS_WorldSimulationValidationUtility` | Profile and vendor supply mapping validation |

## Settlement Simulation

Each discovered settlement tracks:

- Population
- Prosperity (0–100, persisted)
- Supply categories: Food, Water, Fuel, BuildingMaterials, IndustrialMaterials, Tools, TradeGoods
- Demand and production entries (profile-driven)

Prosperity is derived from food fill %, average supply fill %, and production vs demand %.

## Region Simulation

Regions track metadata only:

- Region id
- Food, wildlife, mining, and industry potential
- Discovery state synced from `CCS_RegionService`

Region simulation does **not** generate resources.

## Player Economy Integration

`CCS_WorldSimulationService` listens to `CCS_VendorService.VendorTransactionCompleted`:

| Player action | Supply impact |
|---------------|---------------|
| Sell fish, meat, jerky, dried fish | Increase Food |
| Sell corn, beans, potatoes, wheat | Increase Food |
| Sell wood, lumber | Increase Building Materials (+ partial Fuel for wood/lumber) |
| Sell charcoal | Increase Fuel |
| Sell ore, refined iron, nails, tools | Increase Industrial Materials / Tools |
| Buy items | Decrease mapped supply category |

Vendor routes in the profile map vendor ids to settlement ids.

## Save / Load

`CCS_SaveWorldSimulationData` persists settlement and region simulation arrays through `CCS_SaveService`.

## Playtest

World Simulation checklist group (steps 127–134):

1. Discover settlement
2. Sell food
3. Verify food supply increased
4. Sell industry goods
5. Verify industry supply increased
6. Verify prosperity increased
7. Save
8. Verify simulation restored after load

## Bootstrap Content

- Profile: `Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset`
- Trading post settlement simulation entry
- Region entries: Pine Ridge Forest, Broken Creek, Iron Ridge Mine, Frontier Trading Post Region
- General store vendor route

Batch setup: `CCS.Modules.WorldSimulation.Editor.CCS_WorldSimulationBootstrapSetup.ExecuteBatch`

## Frontier World Simulation Loop

```
Gather Resources
      ↓
Trade Goods
      ↓
Settlement Supply Changes
      ↓
Prosperity Changes
      ↓
Frontier Evolves
```
