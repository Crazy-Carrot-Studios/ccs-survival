# CCS Settlements Module

**Module ID:** `ccs.survival.settlements`  
**Milestone:** 4.4.0 — Settlement housing capacity + markers; 4.3.0 — NPC service representatives; 4.1.0 — NPC identity on population placeholders; 4.0.0 population presence  
**Milestone:** 3.9.0 — Settlement visual growth (stage markers/labels driven by growth stage)  
**Milestone:** 3.8.0 — Visible business presence (primitive markers/labels driven by business activation)  
**Milestone:** 3.7.0 — Frontier businesses foundation (activation from population, prosperity, growth, reputation)  
**Milestone:** 3.6.0 — Population foundation (workforce categories, growth, capacity, save/load)  
**Milestone:** 3.5.0 — Route risk and freight bonus (risk rating, base/distance multipliers, reward utility)  
**Milestone:** 3.4.0 — Trade routes and freight contracts (discovery, active, usage; outbound regional freight)  
**Milestone:** 3.3.0 — Multi-settlement frontier network (4 independent settlements)

**Milestone:** 3.2.0 — Settlement growth foundation (Outpost → TradingPost active)  
**Author:** James Schilz

## Purpose

Generic settlement framework for frontier service locations beyond the player homestead:

- Towns
- Trading posts
- Mining camps
- Rail camps
- Ranches
- Forts

No NPC AI, dialogue, quest systems, or final town art.

## Settlement Service Hub Loop

```text
Discover Trading Post
  ↓
Use Store / Stable / Gunsmith / Blacksmith / Bank / Land Office
  ↓
Access Economy + Industry Services
  ↓
Expand Frontier Progression
```

## Multi-settlement network (3.3.0)

| Settlement | Region specialization | Contract board focus |
|------------|----------------------|----------------------|
| Frontier Trading Post | FrontierMixed | Mixed frontier supply |
| Pine Ridge Camp | Timber | Lumber, poles, charcoal |
| Broken Creek Farmstead | Agriculture | Corn, wheat, potatoes, milk |
| Iron Ridge Mining Camp | Mining | Iron ore, coal, refined iron |

Each settlement maintains independent discovery, prosperity, supply, growth stage, reputation, and simulation state.

Trade route metadata (no transport simulation):

- `CCS_TradeRouteDefinition` / `CCS_TradeRouteProfile` / `CCS_TradeRouteSnapshot`
- Risk: `CCS_TradeRouteRiskLevel` (Safe, Low, Moderate active; Dangerous/Severe placeholders)
- Rewards: `baseFreightMultiplier`, `distanceMultiplier`, `CCS_TradeRouteRewardModifierUtility`
- Placeholders: `preferredWagonRequirementPlaceholder`, `routeConditionPlaceholder`
- Runtime: `CCS_TradeRouteService` (discovery, active, usage count)
- Persisted usage through `CCS_SaveTradeRoutesWorldData` (risk fields are profile data)

Route risk bootstrap batch:

```text
CCS.Modules.Settlements.Editor.CCS_TradeRoutesRiskFoundationBootstrapSetup.ExecuteBatch
```

Playtest group: **Route Risk / Freight** — shortcut **Ctrl+Shift+Q**.

Freight bootstrap batch:

```text
CCS.Modules.Settlements.Editor.CCS_TradeRoutesFreightFoundationBootstrapSetup.ExecuteBatch
```

Playtest group: **Trade Routes / Freight** — shortcut **Ctrl+Shift+F**.

Multi-settlement bootstrap batch:

```text
CCS.Modules.Settlements.Editor.CCS_MultiSettlementFoundationBootstrapSetup.ExecuteBatch
```

Playtest group: **Multi-Settlement** — shortcut **Ctrl+Shift+N**.

## Population presence (4.0.0)

| Type | Role |
|------|------|
| `CCS_PopulationPresenceProfile` | Workforce anchor catalog per settlement |
| `CCS_PopulationPresenceAnchor` | Spawns capped idle capsule placeholders |
| `CCS_PopulationPlaceholderActor` | Category-colored worker with NPC name/role/settlement affiliation label (4.8.0) |
| `CCS_PopulationPresenceLabel` | Dev label: category, source count, visible count |
| `CCS_PopulationPresenceService` | Refreshes actors from population snapshots |

Bootstrap anchors: Trading Post (Merchants, Laborers); Broken Creek (Farmers, Ranchers); Iron Ridge (Miners, Laborers); Pine Ridge (Lumber Workers, Laborers).

Visual state derives from world simulation population — no separate save section.

Milestone **4.8.0** — placeholder labels show settlement affiliation (line three) and debug affiliation/loyalty HUD via `CCS_NpcAffiliationService`.

Bootstrap: `CCS.Modules.Settlements.Editor.CCS_PopulationPresenceFoundationBootstrapSetup.ExecuteBatch`

Playtest: **Population Presence** — **Ctrl+Shift+X**

## Settlement housing (4.4.0)

Settlement-owned housing adds population capacity and dev-readable world markers:

- `CCS_SettlementHousingProfile` / `CCS_SettlementHousingService`
- Bootstrap housing: Boarding House (Trading Post), Farmhouse (Broken Creek), Worker Cabin (Pine Ridge), Mining Barracks (Iron Ridge)
- Total capacity = base population capacity + active housing capacity
- Persisted on `CCS_SettlementSimulationState.housingStates`
- Debug HUD shows base / housing / total capacity and active housing names

**Settlement Housing Loop:**

```text
Population Grows → Housing Capacity Matters → Housing Markers Show Settlement Life → Future NPC Homes / Schedules Ready
```

Bootstrap: `CCS_SettlementHousingFoundationBootstrapSetup.ExecuteBatch`

Playtest: **Settlement Housing** — **Ctrl+Alt+H**

## NPC identity on placeholders (4.1.0)

Population anchors call `CCS_NpcRuntimeBridge` after spawning actors. Each slot gets a stable `npcIdentityId`, display name, role, settlement id, optional business id, and workforce category. See `Assets/CCS/Modules/NPCs/Documentation/CCS_Npc_Module.md`.

Playtest: **NPC Identity** — **Ctrl+Shift+E**

## NPC service representatives (4.3.0)

Active businesses assign named representatives from population placeholders (or synced anchors at service points). Labels use **name + title** for representatives; workers keep `Name — Role`. Interaction routes through existing `CCS_SettlementServicePoint` resolver; service cubes remain fallback.

See `Assets/CCS/Modules/NPCs/Documentation/CCS_Npc_Module.md`.

Playtest: **NPC Service Representatives** — **Ctrl+Alt+R**

## NPC schedule and movement (4.6.0 / 4.5.0)

Placeholder labels may include a dev schedule debug line (`block | schedule id | target kind`) via `CCS_NpcScheduleLabelBridge`. `CCS_PopulationPresenceRuntimeBridge.TryGetFirstAnchorPositionForSettlement` supplies settlement-center targets for break/leisure blocks. Movement remains transform-based with no NavMesh.

Playtest: **NPC Schedule** — **Ctrl+Alt+S** · **NPC Movement** — **Ctrl+Alt+M** · **NPC Activity** — **Ctrl+Alt+A** · **NPC Affiliations** — **Ctrl+Alt+F**

See `Assets/CCS/Modules/NPCs/Documentation/CCS_Npc_Module.md`.

**Service Representative Loop:**

```text
Business Activates → Representative Assigned → Player Talks To Named NPC → Existing Service Opens → Town Feels Human
```

## Settlement visual growth (3.9.0)

| Type | Role |
|------|------|
| `CCS_SettlementVisualGrowthMarkerType` | Camp, supply crates, signs, service hub, hitching rail, placeholders |
| `CCS_SettlementVisualGrowthProfile` | Anchor catalog per settlement and required growth stage |
| `CCS_SettlementVisualGrowthService` | Refreshes markers from `CCS_SettlementGrowthSnapshot` |
| `CCS_SettlementVisualGrowthRuntimeBridge` | Safe anchor registry when services are missing |
| `CCS_SettlementVisualGrowthAnchor` | World anchor with marker + label children |

Bootstrap scene anchors: Trading Post (Outpost + TradingPost + future placeholders); Broken Creek, Iron Ridge, Pine Ridge (Outpost + TradingPost markers).

Visual state is **not** saved separately — restored from settlement growth simulation on load.

Bootstrap: `CCS.Modules.Settlements.Editor.CCS_SettlementVisualGrowthFoundationBootstrapSetup.ExecuteBatch`

Playtest: **Settlement Visual Growth** — **Ctrl+Shift+Z**

**Settlement Visual Growth Loop:**

```text
Complete Contracts → Settlement Grows → Stage Markers Activate → World Visibly Changes
```

## Business presence (3.8.0)

Primitive labeled markers at each settlement business anchor; visuals derive from `CCS_BusinessSnapshot` (no separate save section). Service points linked by business type tint from presence status. Independent from visual growth zone markers.

Bootstrap: `CCS.Modules.Settlements.Editor.CCS_BusinessPresenceFoundationBootstrapSetup.ExecuteBatch`

Playtest: **Business Presence** — **Ctrl+Shift+V**

## Businesses (3.7.0)

| Type | Role |
|------|------|
| `CCS_BusinessType` | General Store, Stable, Gunsmith, Bank, Farm Supply, Mining Supplier, Lumber Yard, Contract Office, Blacksmith |
| `CCS_BusinessProfile` | Threshold definitions and per-settlement catalogs |
| `CCS_BusinessService` | Activation events and snapshot resolver |
| `CCS_BusinessRuntimeBridge` | Service point gating from simulation state |

See [CCS_Businesses_Module.md](CCS_Businesses_Module.md).

Bootstrap:

```text
CCS.Modules.Settlements.Editor.CCS_FrontierBusinessesFoundationBootstrapSetup.ExecuteBatch
```

Playtest group: **Businesses** — shortcut **Ctrl+Shift+J**.

**Business Loop:**

```text
Population → Businesses Open → Services Expand → Prosperity Improves → Settlement Grows
```

## Population (3.6.0)

| Type | Role |
|------|------|
| `CCS_SettlementPopulationCategory` | Farmers, Ranchers, Miners, Lumber Workers, Merchants, Laborers |
| `CCS_SettlementPopulationProfile` | Growth modifiers, capacity, per-settlement starting population |
| `CCS_SettlementPopulationUtility` | Growth, workforce distribution, validation (World Simulation assembly) |
| `CCS_SettlementPopulationSnapshot` | Query snapshot for HUD and playtest |

Bootstrap batch:

```text
CCS.Modules.Settlements.Editor.CCS_SettlementPopulationFoundationBootstrapSetup.ExecuteBatch
```

Playtest group: **Population** — shortcut **Ctrl+Shift+K**.

**Population Loop:**

```text
Supply Settlement → Population Grows → Workforce Expands → Production Improves → Settlement Develops
```

## Settlement growth (3.2.0 / 3.6.0)

| Type | Role |
|------|------|
| `CCS_SettlementGrowthStage` | Outpost, TradingPost (active), FrontierTown, EstablishedTown (placeholders) |
| `CCS_SettlementGrowthDefinition` | Per-stage thresholds (prosperity, food %, industrial %, contracts, region placeholder) |
| `CCS_SettlementGrowthProfile` | Definition catalog + per-settlement starting stage |
| `CCS_SettlementGrowthSnapshot` | Runtime query snapshot |
| `CCS_SettlementGrowthUtility` | Validation, stage resolution, progress % |
| `CCS_SettlementGrowthDebugHud` | Prosperity, supply health, stage, next-stage progress |
| `CCS_SettlementGrowthRuntimeBridge` | Forwards growth events to location visuals |

`CCS_SettlementService` exposes `TryGetSettlementGrowthStage`, `TryGetGrowthSnapshot`, and `SettlementGrowthChanged`.

Frontier Trading Post (`ccs.survival.settlement.tradingpost`) starts at **Outpost**. **TradingPost** requires population ≥ 50, prosperity ≥ 35, food supply ≥ 25%, and ≥ 1 completed contract.

Growth state persists on `CCS_SettlementSimulationState` (current/previous stage, progress %, completed contract count) through world simulation save/load.

**Settlement Growth Loop:**

```text
Complete Contracts → Improve Supply + Prosperity → Settlement Growth Progress → New Growth Stage → Future Services / Expansion
```

Bootstrap batch:

```text
CCS.Modules.Settlements.Editor.CCS_SettlementGrowthFoundationBootstrapSetup.ExecuteBatch
```

Playtest group: **Settlement Growth** — shortcut **Ctrl+Shift+G**.

## Bootstrap test settlement

**Object:** `CCS_TestTradingPost` in `SCN_CCS_Survival_Bootstrap.unity`

**Definition:** `Assets/CCS/Survival/Content/Settlements/CCS_Settlement_TestTradingPost.asset`

Service points:

| Service | Type | Routing | Availability |
|---------|------|---------|--------------|
| General Store | `GeneralStore` | `CCS_Vendor_GeneralStore` | Always |
| Stable | `Stable` | `CCS_Vendor_FrontierStable` | Always |
| Gunsmith | `Gunsmith` | `CCS_Vendor_FrontierGunsmith` | Always |
| Blacksmith | `Blacksmith` | Industry summary (`CCS_SettlementIndustryServiceHud`) | When industry service exists |

## Service activation results

`CCS_SettlementServiceRouteResolver` returns structured results:

| Route | Status | Behavior |
|-------|--------|----------|
| Vendor | Succeeded | `CCS_VendorService` + `CCS_VendorDebugHud` |
| Industry | Succeeded | `CCS_SettlementIndustryServiceHud` summary |
| Placeholder | Succeeded | `CCS_SettlementDebugMessageHud` message |
| Disabled | Disabled | Safe message; no service mutation |
| Unavailable | Unavailable | Safe message; requirements not met |
| Unknown | UnknownRoute | Safe fallback message |

## Availability flags

Each `CCS_SettlementServicePoint` supports:

- `isAvailable` — hard disable
- `unavailableReason` — player-facing message
- `requiredSettlementDiscovered` — gate until discovery
- `requiredCampTier` — future camp tier placeholder (-1 = none)

Blacksmith availability is also tied to `CCS_IndustryService` initialization.

## Interaction flow

```text
Look at service point
  ↓
Interact (F)
  ↓
Route resolver checks availability
  ↓
Vendor → economy debug panel
Industry → forge / workstation summary (no auto-craft)
Placeholder → settlement debug message
```

## Discovery / map placeholder

`CCS_SettlementService` tracks per settlement:

- `settlementId`
- `displayName`
- `settlementType`
- `discovered`
- `position`

Persisted in unified save under `settlements.discoveries`. No map UI yet.

Activation events include `RouteType`, `ActivationStatus`, and `Message` for playtest validation.

## Runtime types

| Type | Role |
|------|------|
| `CCS_SettlementType` | Settlement archetype enum |
| `CCS_SettlementServicePointType` | Service point enum |
| `CCS_SettlementServiceRouteType` | Activation route enum |
| `CCS_SettlementServiceActivationStatus` | Activation result status |
| `CCS_SettlementServiceActivationResult` | Structured activation outcome |
| `CCS_SettlementDefinition` | ScriptableObject settlement catalog entry |
| `CCS_SettlementProfile` | Module profile with definition list |
| `CCS_SettlementService` | Discovery state + service point events |
| `CCS_SettlementServiceRouteResolver` | Availability + routing logic |
| `CCS_SettlementLocation` | World root with proximity discovery |
| `CCS_SettlementServicePoint` | Interactable service routing |
| `CCS_SettlementIndustryServiceHud` | Blacksmith / industry debug summary |
| `CCS_SettlementSnapshot` | Runtime discovery record |
| `CCS_SettlementRuntimeBridge` | Service registry resolver |
| `CCS_SettlementValidationUtility` | Profile validation |

## Reputation integration (2.7.0+)

Optional bind to `CCS_ReputationService`:

- `TryGetSettlementReputation(settlementId, out standing)` — current value and tier
- `SettlementReputationChanged` — forwarded settlement-scope changes

### Service access (2.8.0)

`CCS_SettlementServiceRouteResolver.EvaluateAvailability` calls `CCS_ServiceAccessEvaluationUtility` before routing.

`CCS_SettlementServicePoint.EvaluateServiceAccess` supports profile-driven rules:

- Minimum reputation tier / value (active)
- Required discovered settlement
- Camp tier / land claim placeholders
- Enabled / disabled rules

Access results: Allowed, DeniedReputation, DeniedUnavailable, DeniedDisabled, MissingRequirement.

Default content keeps core services allowed at Neutral. Blacksmith advanced access may require Trusted (non-essential).

Settlement debug HUD shows access result and missing requirement message.

See `Assets/CCS/Modules/Reputation/Documentation/CCS_Reputation_Module.md`.

## Bootstrap batch

```text
CCS.Modules.Settlements.Editor.CCS_FrontierSettlementBootstrapSetup.ExecuteBatch
```

## Input policy

Dev hotkeys use `CCS_DevHotkeyUtility` / New Input System only. Legacy `UnityEngine.Input` is banned.
