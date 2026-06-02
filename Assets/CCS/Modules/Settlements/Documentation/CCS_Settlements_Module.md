# CCS Settlements Module

**Module ID:** `ccs.survival.settlements`  
**Milestone:** 1.8.1 — Settlement Services Polish + Blacksmith Routing  
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
Use Store / Stable / Gunsmith / Blacksmith
  ↓
Access Economy + Industry Services
  ↓
Expand Frontier Progression
```

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

## Bootstrap batch

```text
CCS.Modules.Settlements.Editor.CCS_FrontierSettlementBootstrapSetup.ExecuteBatch
```

## Input policy

Dev hotkeys use `CCS_DevHotkeyUtility` / New Input System only. Legacy `UnityEngine.Input` is banned.
