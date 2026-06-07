# CCS World Simulation Module

Milestone **5.1.0** — **Settlement event state** persists on `CCS_SettlementSimulationState.activeSettlementEvent` (active event id, type, start day/hour, duration, settlement id). Profile wired on `CCS_WorldSimulationProfile.settlementEventProfile`. `CCS_SettlementEventService` evaluates generation on discovery and hour changes; prosperity recalculation and contract completion apply temporary event modifiers through `CCS_SettlementEventRuntimeBridge`.

Milestone **4.8.0** — **NPC affiliation state** persists on `CCS_SettlementSimulationState.npcAffiliationStates` (settlement, region, business, workforce, representative flag, loyalty). Profile wired on `CCS_WorldSimulationProfile.settlementNpcAffiliationProfile`. `CCS_NpcAffiliationService` auto-assigns affiliations when identities are created.

Milestone **4.7.0** — **NPC activity state** persists on `CCS_SettlementSimulationState.npcActivityStates` (current activity type, last evaluated hour). Profile wired on `CCS_WorldSimulationProfile.settlementNpcActivityProfile`. `CCS_NpcActivityService` derives activities from schedule blocks and movement status.

Milestone **4.6.0** — **NPC schedule state** persists on `CCS_SettlementSimulationState.npcScheduleStates` (active schedule id, current block type, target kind/id, last evaluated hour). Profile wired on `CCS_WorldSimulationProfile.settlementNpcScheduleProfile`. `CCS_NpcScheduleService` evaluates daily blocks and integrates with movement target selection.

Milestone **4.5.0** — **NPC movement state** persists on `CCS_SettlementSimulationState.npcMovementStates` (movement status, target/workplace/home ids). Profile wired on `CCS_WorldSimulationProfile.settlementNpcMovementProfile`. `CCS_NpcMovementService` moves placeholders toward schedule-selected anchors.

Milestone **4.4.0** — **Settlement housing** persists on `CCS_SettlementSimulationState.housingStates` (housing id, settlement id, active flag, capacity contribution, required growth stage). Profile wired on `CCS_WorldSimulationProfile.settlementHousingProfile`. `CCS_SettlementHousingService` syncs activation on discovery/growth changes; total population capacity = base + active housing capacity.

Milestone **4.3.0** — **NPC service representatives** persist on `CCS_SettlementSimulationState.npcServiceRepresentativeStates` (representative id, business id, service point id, assigned identity, active flag). Profile wired on `CCS_WorldSimulationProfile.settlementNpcServiceRepresentativeProfile`. `CCS_NpcServiceRepresentativeService` syncs on business activation events and restores presentation after load.

Milestone **4.1.0** — **NPC identity** for population placeholders persists on `CCS_SettlementSimulationState.npcIdentityStates` (names, roles, settlement/business ids, workforce category). Profile wired on `CCS_WorldSimulationProfile.settlementNpcIdentityProfile`. `CCS_NpcIdentityService` resolves identities through `CCS_NpcRuntimeBridge`.

Milestone **4.0.0** — **Population presence** placeholder actors derive visible counts from `CCS_SettlementPopulationSnapshot` via `CCS_PopulationPresenceValidationUtility`. Profile wired on `CCS_WorldSimulationProfile.settlementPopulationPresenceProfile`. Population changes refresh anchors through `CCS_PopulationPresenceService`.

Milestone **3.9.0** — **Settlement visual growth** markers derive state from `CCS_SettlementGrowthSnapshot` via `CCS_SettlementVisualGrowthValidationUtility`. Profile wired on `CCS_WorldSimulationProfile.settlementVisualGrowthProfile`. Growth stage changes refresh anchors through `CCS_SettlementVisualGrowthService`.

Milestone **3.8.0** — **Business presence** markers in bootstrap scene derive visual state from `CCS_BusinessSnapshot` via `CCS_BusinessPresenceValidationUtility`. Profile wired on `CCS_WorldSimulationProfile.settlementBusinessPresenceProfile`.

Milestone **3.7.0** — Settlement **business activation** on `CCS_SettlementSimulationState.businessStates`. `CCS_BusinessValidationUtility` evaluates catalog entries when population, prosperity, or growth stage changes. `CCS_BusinessService` raises activation events; profile wired on `CCS_WorldSimulationProfile.settlementBusinessProfile`.

Milestone **3.6.0** — Settlement **population simulation** on `CCS_SettlementSimulationState` (total, capacity, growth rate, stability, workforce breakdown). `CCS_SettlementPopulationUtility` applies growth from contracts, prosperity, food supply health, and reputation tier. Persisted through `CCS_SaveWorldSimulationData`. Wired to settlement growth population gates.

Milestone **3.5.0** — Freight completion unchanged for prosperity/supply; optional conservative bonus destination reputation from route risk (`CCS_TradeRouteRewardModifierUtility.ResolveBonusReputation`). Route usage counts still persist via `CCS_TradeRouteService`; risk multipliers are profile-only.

Milestone **3.4.0** — Freight contract completion applies destination prosperity and supply through `HandleContractCompleted` (no population changes). Trade route usage is tracked separately via `CCS_TradeRouteService`.

Milestone **3.2.0** — Settlement growth evaluation after contract completion, major supply updates, prosperity recalculation, and save/load restore. Growth profile on `CCS_WorldSimulationProfile.settlementGrowthProfile`.

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
- Business activation (`businessStates`, persisted)
- Prosperity (0–100, persisted)
- Supply categories: Food, Water, Fuel, BuildingMaterials, IndustrialMaterials, Tools, TradeGoods
- Demand and production entries (profile-driven)
- Growth: `currentGrowthStage`, `previousGrowthStage`, `growthProgressPercent`, `completedContractsCount`

Prosperity is derived from food fill %, average supply fill %, and production vs demand %.

`CCS_WorldSimulationService` evaluates settlement growth via `CCS_SettlementGrowthUtility` and raises `SettlementGrowthChanged` when stage or progress updates.

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
Growth content: `CCS.Modules.Settlements.Editor.CCS_SettlementGrowthFoundationBootstrapSetup.ExecuteBatch`

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
