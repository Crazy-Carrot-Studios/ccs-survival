# CCS Reputation Module

Milestone **2.7.0** — generic reputation and service trust foundation for frontier settlements.

## Purpose

Tracks player standing by scope (settlement, region, service, future faction, global) for future discounts, access rules, law systems, faction hooks, and quests. Western-specific reputation naming lives in Survival content assets under `Assets/CCS/Survival/Content/Reputation/`.

## Frontier Trust Loop

Discover Settlement  
↓  
Trade / Pay Obligations  
↓  
Settlement Trust Changes  
↓  
Future Service Access / Discounts / Law Hooks  

## Runtime Types

| Type | Role |
|------|------|
| `CCS_ReputationDefinition` | ScriptableObject standing track (scope, target id, min/max/default) |
| `CCS_ReputationProfile` | Profile catalog, event hook flags, conservative deltas |
| `CCS_ReputationScopeType` | Settlement, Region, Service, FutureFaction, Global |
| `CCS_ReputationTier` | Hostile, Distrusted, Neutral, Trusted, Honored |
| `CCS_ReputationStanding` | Runtime query result (value + tier) |
| `CCS_ReputationSnapshot` | Serializable standing for save/load |
| `CCS_ReputationEvent` | Last event record for debug HUD |
| `CCS_ReputationChangedEventArgs` | Event payload when standing changes |
| `CCS_ReputationService` | Standings, event application, save/restore |
| `CCS_ReputationRuntimeBridge` | Service resolution for HUD and settlement integration |
| `CCS_ReputationValidationUtility` | Shared profile/content validation |
| `CCS_ReputationDebugHud` | Dev-only settlement trust summary |

## Rules (2.7.0)

- Default range **-100 to +100**; new standings start **Neutral (0)**.
- **Settlement** scope is active; Region/Service placeholders exist; factions deferred.
- Profile-enabled hooks only — no forced reputation changes when flags are off.
- Conservative deltas: goods sold +2, loan repaid +3, upkeep paid +2, failed upkeep -1 (profile tunable).
- No service lockouts, final UI, quests, law, or NPC AI in this milestone.

## Settlement Integration

`CCS_SettlementService` exposes:

- `TryGetSettlementReputation(settlementId, out CCS_ReputationStanding standing)`
- `SettlementReputationChanged` — forwarded from `CCS_ReputationService` for settlement scope

Service points do not restrict access in 2.7.0.

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

Run **`CCS_ReputationFoundationBootstrapSetup.ExecuteBatch`** for Frontier Trading Post trust content, profile host wiring, and playtest steps.

## Playtest

Reputation playtest group (`CCS_PlaytestStepGroup.Reputation`) covers sell trust increase, obligation trust change, save, and load verification.

Shortcut: **Ctrl+Shift+T** (`TryPlaytestReputationFoundationShortcut`).

## Deferred

- Factions and faction-wide standing
- Quest reputation gates
- Law/crime and bounty systems
- Service lockouts and dynamic pricing
- Final reputation UI
