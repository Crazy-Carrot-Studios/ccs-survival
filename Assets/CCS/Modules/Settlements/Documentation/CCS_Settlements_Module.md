# CCS Settlements Module

**Module ID:** `ccs.survival.settlements`  
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
- Runtime: `CCS_TradeRouteService` (discovery, active, usage count)
- Persisted through `CCS_SaveTradeRoutesWorldData`

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

## Settlement growth (3.2.0)

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

Frontier Trading Post (`ccs.survival.settlement.tradingpost`) starts at **Outpost**. **TradingPost** requires prosperity ≥ 35, food supply ≥ 25%, and ≥ 1 completed contract.

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
