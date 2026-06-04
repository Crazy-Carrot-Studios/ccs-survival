# CCS Contracts Module

Milestone **3.5.0** — Freight contracts with `linkedTradeRouteId` scale trade-dollar rewards by route base/distance and risk multipliers (`CCS_TradeRouteRewardModifierUtility`). `CCS_ContractCompletionResult` and `CCS_ContractDebugHud` expose base/route/risk/final reward breakdown. Local contracts unchanged; missing route service falls back to base reward.

Milestone **3.4.0** — **FreightDelivery** contracts with origin/destination settlement boards, wagon-cargo-first completion (`CCS_ContractFreightUtility`), optional player-inventory fallback, destination prosperity/supply rewards, and linked trade route usage tracking.

Milestone **3.2.0** — Contract completion increments settlement `completedContractsCount` and triggers world simulation growth evaluation (rewards unchanged; no duplicate prosperity logic).

Milestone **3.0.0** — Frontier Contracts Foundation

## Purpose

Generic settlement **contract / job** system for item-delivery requests in exchange for trade dollars, reputation, and world-simulation prosperity/supply rewards.

## Runtime layout

| Area | Responsibility |
|------|----------------|
| `Definitions/CCS_ContractDefinition` | ScriptableObject contract catalog entry |
| `Profiles/CCS_ContractProfile` | Registered profile on `CCS_SurvivalGameplayServiceHost` |
| `Services/CCS_ContractService` | Accept, complete, save/load contract instances |
| `Services/CCS_ContractRuntimeBridge` | Resolves active contract service from registry |
| `UI/CCS_ContractDebugHud` | Debug accept/complete panel (no final UI) |
| `Validation/CCS_ContractValidationUtility` | Profile/definition validation |

## Contract types

- General Store Supply
- Gunsmith Supply
- Stable Supply
- Trading Post Supply
- Land Office Supply
- Freight Delivery (3.4.0+)

## Freight delivery (3.4.0 / 3.5.0)

- Accept at **source** settlement contract board
- Complete at **destination** contract board
- `GetSettlementBoardContracts` lists local supply, outbound freight, and inbound accepted freight
- Rewards apply at destination; optional `OriginReputationGain` at source
- Bootstrap: `CCS_TradeRoutesFreightFoundationBootstrapSetup.ExecuteBatch`
- Route risk bootstrap: `CCS_TradeRoutesRiskFoundationBootstrapSetup.ExecuteBatch`

**Freight loop (3.4.0):** Produce Regional Goods → Load Wagon → Travel Route → Deliver To Destination → Increase Prosperity + Reputation

**Route risk freight loop (3.5.0):**

```text
Accept Freight → Assess Route Risk → Load Wagon → Deliver Goods → Earn Risk-Adjusted Reward
```

## Requirements

Each contract supports:

- Item id
- Quantity
- Optional settlement restriction (definition-level and per-requirement)

## Rewards

Conservative values only:

- Trade Dollars (`CCS_CurrencyService`) — freight with linked route: `base × routeMultiplier × riskMultiplier` (non-negative)
- Reputation gain (`CCS_ReputationService.TryApplyContractReward`)
- Settlement prosperity + supply category (`CCS_WorldSimulationService.HandleContractCompleted`)
- Settlement growth progress (`completedContractsCount` + `EvaluateSettlementGrowth`)

## Settlement integration

Service route: **`ContractBoard`**

- `CCS_SettlementServiceRouteType.ContractBoard`
- `CCS_SettlementServicePointType.ContractBoard`
- Scene object: `CCS_TestTradingPost_ContractBoard`
- Interaction opens `CCS_ContractDebugHud.ShowBoard`

## Starter catalog

| Board | Contracts |
|-------|-----------|
| General Store | Lumber, Corn, Potato delivery |
| Stable | Feed, Milk delivery |
| Gunsmith | Iron Ore, Refined Iron, Charcoal delivery |
| Trading Post | Mixed Frontier Supply (hide + cordage) |

Content: `Assets/CCS/Survival/Content/Contracts/`  
Profile: `Assets/CCS/Survival/Profiles/Contracts/CCS_DefaultContractProfile.asset`

## Save / load

`CCS_SaveContractsWorldData` stores `CCS_ContractSnapshot[]` (definition id, state, accepted settlement id).

## Playtest

Group: **Contracts**  
Shortcut: **Ctrl+Shift+C** (`TryPlaytestContractsFoundationShortcut`)

Steps cover discover → accept → gather → complete → verify rewards → save/load stability.

## Bootstrap

```text
CCS_ContractsFoundationBootstrapSetup.ExecuteBatch
```

Creates content assets, wires bootstrap host profile, adds contract board to bootstrap scene, and inserts playtest steps.

## Validation

Registered via `CCS_ContractsValidationRegistration` on `CCS_SurvivalValidationPipeline`.
