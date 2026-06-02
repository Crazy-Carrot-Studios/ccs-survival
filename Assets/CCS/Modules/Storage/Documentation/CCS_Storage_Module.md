# CCS Storage Module

**Milestone:** 1.1.2 — Storage Container Foundation  
**Author:** James Schilz (Developer)  
**Date:** 2026-06-01

## Purpose

Turn the crafted **Storage Crate** item into a usable primitive storage container with persistent inventory. Players gather resources, craft a crate at the workbench, place or interact with the crate, move items in/out, and save/load world crate state through the unified save file.

Primitives only — no real art assets, multiplayer, or item database editor in this milestone.

## Frontier homestead storage (1.4.1)

| Container | Definition id | Camp contribution |
|-----------|---------------|-------------------|
| Supply Crate | `ccs.survival.storage.frontier.supplycrate` | Yes (kit placement) |
| Trapper Chest | `ccs.survival.storage.frontier.trapperchest` | Yes |

`CCS_StorageContainerDefinition` supports `placeableKitItem` and `contributesToCampTier`. Placed instances within camp radius advance tier to **FrontierCamp** via `CCS_FrontierHomesteadStructureService` + `CCS_CampService`.

## Gameplay flow

1. Gather resources and craft `ccs.survival.item.progression.storagecrate` at the workbench.
2. Place or interact with `PF_CCS_PrimitiveStorageCrate` (dev **F2** near player).
3. Open the crate via interaction (service tracks active container).
4. Move items using dev helpers (**F1** deposit, **Shift+F1** withdraw) until UI exists.
5. **F5** save / **F9** load — crate position, rotation, and slot contents restore; active open state clears after load.

## Runtime types

| Type | Role |
|------|------|
| `CCS_StorageContainerDefinition` | Container id, display name, slot count, optional max weight, prefab, debug logging |
| `CCS_StorageContainer` | World instance inventory, open/close, add/remove, capture/restore state |
| `CCS_StorageContainerInteractable` | Interaction handoff to `CCS_StorageService` |
| `CCS_StorageService` | Register containers, active container, transfers, world save capture/restore, events |
| `CCS_StorageRuntimeBridge` | Resolve services from `CCS_RuntimeHost` |
| `CCS_StorageEventArgs` | Container id, instance id, display name, world position, success, message |
| `CCS_StorageProfile` | Default definition and service debug flags |

## Events

- `StorageContainerOpened`
- `StorageContainerClosed`
- `StorageItemAdded`
- `StorageItemRemoved`
- `StorageStateRestored`

## Save data shape

Root `CCS_SaveData.storage` (`CCS_SaveStorageWorldData`):

- `containers[]`
  - `containerDefinitionId`
  - `instanceId`
  - `displayName`
  - `positionX/Y/Z`
  - `rotationX/Y/Z/W`
  - `slots[]` → `itemId`, `quantity`

## Dev hotkeys (playtest harness)

| Key | Action |
|-----|--------|
| **F2** | Place primitive storage crate near player (or open nearest / toggle close if already active) |
| **F1** | Move first player inventory item into active/open crate |
| **Shift+F1** | Move first crate item back to player |
| **F5 / F9** | Unified save / load (validates persistence for playtest step) |

## Content paths

| Asset | Path |
|-------|------|
| Definition | `Assets/CCS/Survival/Content/Storage/Primitive/CCS_PrimitiveStorageCrateDefinition.asset` |
| Prefab | `Assets/CCS/Survival/Content/Storage/Primitive/Prefabs/PF_CCS_PrimitiveStorageCrate.prefab` |
| Profile | `Assets/CCS/Survival/Profiles/Storage/CCS_DefaultStorageProfile.asset` |
| Test object | `CCS_TestStorageCrate` in `SCN_CCS_Survival_Bootstrap` |
| Item | `ccs.survival.item.progression.storagecrate` |

## Composition

`CCS_SurvivalGameplayServiceRegistration` registers `CCS_StorageService` from `CCS_StorageProfile` and binds it to `CCS_SaveService` and playtest listeners.

## Bootstrap batch

```
CCS.Modules.Storage.Editor.CCS_StorageBootstrapSetup.ExecuteBatch
```

## Validation

Registered via `CCS_StorageValidationRegistration` on the survival validation pipeline.
