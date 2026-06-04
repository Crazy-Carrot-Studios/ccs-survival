# CCS Reputation Module

Milestone **2.8.0** — service access rules and vendor buy price modifiers driven by settlement trust.

Milestone **2.7.0** — generic reputation and service trust foundation for frontier settlements.

## Purpose

Tracks player standing by scope (settlement, region, service, future faction, global) for discounts, service access rules, law systems, faction hooks, and quests. Western-specific reputation naming lives in Survival content assets under `Assets/CCS/Survival/Content/Reputation/`.

## Frontier Trust Service Loop

```text
Trade / Pay Obligations
  ↓
Settlement Trust Improves
  ↓
Better Service Terms
  ↓
Future Service Access Rules
```

## Runtime Types

| Type | Role |
|------|------|
| `CCS_ReputationDefinition` | ScriptableObject standing track (scope, target id, min/max/default) |
| `CCS_ReputationProfile` | Profile catalog, event hooks, price modifiers, service access profile |
| `CCS_ServiceAccessRule` | ScriptableObject access requirement for settlement service points |
| `CCS_ServiceAccessProfile` | Catalog of service access rules |
| `CCS_ServiceAccessRequirement` | Min tier/value, discovery, camp tier, land claim placeholders |
| `CCS_ServiceAccessResult` | Allowed / denied access with missing requirement message |
| `CCS_ServiceAccessEvaluationUtility` | Evaluates rules against reputation and settlement context |
| `CCS_ReputationPriceModifierUtility` | Resolves buy/sell modifiers by tier (fallback 1.0) |
| `CCS_ReputationScopeType` | Settlement, Region, Service, FutureFaction, Global |
| `CCS_ReputationTier` | Hostile, Distrusted, Neutral, Trusted, Honored |
| `CCS_ReputationStanding` | Runtime query result (value + tier) |
| `CCS_ReputationSnapshot` | Serializable standing for save/load |
| `CCS_ReputationEvent` | Last event record for debug HUD |
| `CCS_ReputationChangedEventArgs` | Event payload when standing changes |
| `CCS_ReputationService` | Standings, event application, save/restore |
| `CCS_ReputationRuntimeBridge` | Service resolution for HUD and settlement integration |
| `CCS_ReputationValidationUtility` | Shared profile/content/access/modifier validation |
| `CCS_ReputationDebugHud` | Dev-only settlement trust summary |

## Rules (2.8.0)

- **Service access:** profile-driven rules via `CCS_ServiceAccessProfile`; reputation tier/value checks active; camp tier and land claim are placeholders.
- **Settlement growth (3.2.0):** `CCS_ServiceAccessRequirement.minimumGrowthStage` placeholder (-1 = disabled); no core services locked in default content.
- **Default:** core services allowed at Neutral; no rule configured means allowed.
- **Blacksmith advanced placeholder:** optional Trusted tier requirement (non-essential service).
- **Buy price modifiers:** Neutral 1.0, Trusted 0.95, Honored 0.90, Distrusted 1.10, Hostile 1.25 (profile tunable).
- **Sell modifiers:** optional/conservative; disabled by default in default profile.
- **Missing reputation service:** vendor modifiers fall back to 1.0; access evaluation allows when service unavailable.

## Settlement Integration

`CCS_SettlementServiceRouteResolver` evaluates `CCS_ServiceAccessEvaluationUtility` before activation.

`CCS_SettlementServicePoint.EvaluateServiceAccess` returns structured access results.

Activation statuses include `DeniedReputation` and `MissingRequirement`.

## Economy Integration

`CCS_VendorService` binds `CCS_ReputationService`, tracks active settlement id from service points, and applies buy/sell modifiers through `CCS_ReputationPriceModifierUtility`.

`CCS_VendorTransactionResult` includes base unit price, final unit price, reputation modifier, and settlement id.

## Economy / Banking / Upkeep Hooks

Wired in composition when profile flags are enabled:

| Source event | Reputation event |
|--------------|------------------|
| `VendorTransactionCompleted` (sell) | `GoodsSold` |
| `LoanTransactionCompleted` (paid) | `LoanRepaid` |
| `UpkeepTransactionCompleted` (paid) | `UpkeepPaid` |
| `UpkeepTransactionCompleted` (insufficient funds) | `FailedUpkeep` |
| `SettlementDiscovered` | `SettlementDiscovered` |

## Save / Load

`CCS_SaveReputationWorldData` persists standings (definition id, scope, target id, value, tier, last event summary placeholder).

## Bootstrap

Run **`CCS_ReputationFoundationBootstrapSetup.ExecuteBatch`** for Frontier Trading Post trust content (2.7.0).

Run **`CCS_ServiceAccessFoundationBootstrapSetup.ExecuteBatch`** for service access profile, blacksmith rule, price modifier wiring, and playtest steps (2.8.0).

## Playtest

Reputation playtest group covers sell trust increase, obligation trust change, reputation standing, vendor buy modifier, service access check, save/load stability.

Shortcuts: **Ctrl+Shift+T** (reputation foundation), **Ctrl+Shift+Y** (service access foundation).

## Deferred

Factions, quests, law/crime, NPC AI, final UI, aggressive service lockouts, sell price balancing, camp tier and land claim enforcement.
