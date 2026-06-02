# CCS Regions Module

**Module ID:** `ccs.survival.regions`  
**Milestone:** 1.9.0 — Frontier Region Foundation  
**Author:** James Schilz

## Purpose

Generic world-region framework that organizes settlements, resources, wildlife, travel, and future expansion systems.

No procedural generation, final map UI, factions, quests, or NPC AI in 1.9.0.

## Frontier Region Loop

```text
Travel Frontier
  ↓
Enter Region Volume
  ↓
Discover Region
  ↓
Access Regional Settlements + Resources
  ↓
Expand Frontier Reach
```

## Bootstrap regions

**Scene:** `SCN_CCS_Survival_Bootstrap.unity`

| Region | Type | Volume Object |
|--------|------|---------------|
| Pine Ridge Forest | `Forest` | `CCS_RegionVolume_PineRidgeForest` |
| Broken Creek | `Creek` | `CCS_RegionVolume_BrokenCreek` |
| Iron Ridge Mine | `Mine` | `CCS_RegionVolume_IronRidgeMine` |
| Frontier Trading Post Region | `TradingPost` | `CCS_RegionVolume_FrontierTradingPost` |

Content assets live under `Assets/CCS/Survival/Content/Regions/`.

## Features

- Discover region on volume entry
- Persist discovered regions in unified save
- Track current region (`CurrentRegionId`)
- Settlement ownership by region (`settlementIds` on definition)
- Resource metadata tags by region
- `RegionDiscovered`, `RegionEntered`, `RegionExited` events

## Runtime types

| Type | Role |
|------|------|
| `CCS_RegionType` | Region archetype enum |
| `CCS_RegionDefinition` | ScriptableObject region catalog entry |
| `CCS_RegionProfile` | Module profile with definition list |
| `CCS_RegionService` | Discovery state, current region, events |
| `CCS_RegionVolume` | Trigger volume for entry/exit |
| `CCS_RegionSnapshot` | Runtime discovery record |
| `CCS_RegionRuntimeBridge` | Service registry resolver |
| `CCS_RegionValidationUtility` | Profile validation |

## Bootstrap batch

```text
CCS.Modules.Regions.Editor.CCS_FrontierRegionBootstrapSetup.ExecuteBatch
```

## Input policy

Dev hotkeys use `CCS_DevHotkeyUtility` / New Input System only. Legacy `UnityEngine.Input` is banned.
