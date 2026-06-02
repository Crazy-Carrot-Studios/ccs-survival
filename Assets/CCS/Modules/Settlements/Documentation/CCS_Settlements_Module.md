# CCS Settlements Module

**Module ID:** `ccs.survival.settlements`  
**Milestone:** 1.8.0 — Frontier Settlement Expansion  
**Author:** James Schilz

## Purpose

Generic settlement framework for frontier service locations beyond the player homestead:

- Towns
- Trading posts
- Mining camps
- Rail camps
- Ranches
- Forts

No NPC AI, dialogue, quest systems, or final town art in 1.8.0.

## Frontier Settlement Loop

```text
Travel
  ↓
Discover Trading Post
  ↓
Access Services
  ↓
Trade / Buy Gear
  ↓
Expand Frontier Reach
```

## Bootstrap test settlement

**Object:** `CCS_TestTradingPost` in `SCN_CCS_Survival_Bootstrap.unity`

**Definition:** `Assets/CCS/Survival/Content/Settlements/CCS_Settlement_TestTradingPost.asset`

Service points:

| Service | Type | Routing |
|---------|------|---------|
| General Store | `GeneralStore` | `CCS_Vendor_GeneralStore` |
| Stable | `Stable` | `CCS_Vendor_FrontierStable` |
| Gunsmith | `Gunsmith` | `CCS_Vendor_FrontierGunsmith` |
| Blacksmith | `Blacksmith` | Placeholder message (future industry) |

## Interaction flow

```text
Look at service point
  ↓
Interact (F)
  ↓
Vendor-backed → CCS_VendorService + CCS_VendorDebugHud
Placeholder → CCS_SettlementDebugMessageHud
```

## Discovery / map placeholder

`CCS_SettlementService` tracks per settlement:

- `settlementId`
- `displayName`
- `settlementType`
- `discovered`
- `position`

Persisted in unified save under `settlements.discoveries`. No map UI yet.

## Runtime types

| Type | Role |
|------|------|
| `CCS_SettlementType` | Settlement archetype enum |
| `CCS_SettlementServicePointType` | Service point enum |
| `CCS_SettlementDefinition` | ScriptableObject settlement catalog entry |
| `CCS_SettlementProfile` | Module profile with definition list |
| `CCS_SettlementService` | Discovery state + service point events |
| `CCS_SettlementLocation` | World root with proximity discovery |
| `CCS_SettlementServicePoint` | Interactable service routing |
| `CCS_SettlementSnapshot` | Runtime discovery record |
| `CCS_SettlementRuntimeBridge` | Service registry resolver |
| `CCS_SettlementValidationUtility` | Profile validation |

## Bootstrap batch

```text
CCS.Modules.Settlements.Editor.CCS_FrontierSettlementBootstrapSetup.ExecuteBatch
```

## Input policy

Dev hotkeys use `CCS_DevHotkeyUtility` / New Input System only. Legacy `UnityEngine.Input` is banned.
