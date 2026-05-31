# CCS Survival — Save / Load Module

**Milestone:** 0.7.2 — Environment Effects Persistence  
**Module ID:** `ccs.survival.saveload`  
**Namespace:** `CCS.Modules.SaveLoad` (editor: `CCS.Modules.SaveLoad.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Persistence framework complete at **0.6.0**. Debug manual controls complete at **0.6.1**. Inventory and equipment persistence complete at **0.6.2**. Time of day persistence at **0.7.0**. Weather persistence at **0.7.1**. Environment effects persistence at **0.7.2**. Shelter persistence at **0.7.5**. Building catalog persistence at **0.8.0**. Building placed instance restore at **0.8.4**.

---

## Purpose

Provide the **runtime persistence architecture** that future modules register with to capture and restore state. **0.6.0** delivers JSON save files, a saveable registry, service registration, and a development test saveable — without persisting inventory, world resources, building, combat, wildlife, weather, or quests yet.

---

## Save architecture

```text
CCS_ISaveable (module-owned CaptureState / RestoreState)
        ↓
CCS_SaveableRegistry (tracks registered saveables by SaveableId)
        ↓
CCS_SaveLoadService (create / load / enumerate / delete)
        ↓
CCS_SaveGameData (JSON root document)
        ↓
Application.persistentDataPath/CCS_Survival/Saves/{slotId}.json
```

**Critical rule:** Save/load does not reference UI, world resources, building, combat, or quest systems directly. Modules implement `CCS_ISaveable` and register with the service at runtime.

---

## Folder layout

```text
Assets/CCS/Modules/SaveLoad/
  Runtime/
    Interfaces/     → CCS_ISaveable
    Data/           → save game data, metadata, slot data, results
    Services/       → CCS_SaveLoadService, registry, path utility, bridge
    Profiles/       → CCS_SaveLoadProfile
    Events/         → save/load lifecycle events
    Validation/     → runtime profile validation
    Testing/          → debug controller/panel (SaveLoad), persistence harness (Equipment, development only)
  Editor/
    Validation/     → pipeline validator, bootstrap setup, menu
  Documentation/    → this file

Assets/CCS/Survival/Profiles/SaveLoad/
  CCS_DefaultSaveLoadProfile.asset
```

---

## Registry architecture

| Component | Role |
|-----------|------|
| `CCS_SaveableRegistry` | Instance-owned map of `SaveableId → CCS_ISaveable` |
| `RegisterSaveable` | Future modules call during startup |
| `CaptureAllModuleStates` | Builds module payload dictionary during save |
| `RestoreAllModuleStates` | Applies payloads to registered saveables during load (inventory before equipment) |

Future module examples: Building, World Resources, Weather.

Registered at **0.6.2**:

| SaveableId | Module |
|------------|--------|
| `ccs.survival.saveable.inventory.player` | `CCS_PlayerInventoryService` |
| `ccs.survival.saveable.equipment.player` | `CCS_PlayerEquipmentService` |
| `ccs.survival.saveable.timeofday.global` | `CCS_TimeOfDayService` |
| `ccs.survival.saveable.weather.global` | `CCS_WeatherService` |
| `ccs.survival.saveable.shelter.global` | `CCS_ShelterService` |
| `ccs.survival.saveable.environment.global` | `CCS_EnvironmentEffectsService` |
| `ccs.survival.saveable.building.global` | `CCS_BuildingService` |

Each saveable owns its JSON payload format. The service stores payloads keyed by `SaveableId`.

Load order: inventory → equipment → time of day → weather → shelter → environment → building (see `CCS_SaveLoadSaveableIds.ModuleRestoreOrder`).

---

## JSON format

Root document: `CCS_SaveGameData`

| Field | Purpose |
|-------|---------|
| `saveId` | Unique save instance GUID |
| `slotId` | Slot filename identity |
| `timestampUtc` | ISO-8601 UTC save time |
| `version` | Application/bundle version at save time |
| `profileVersion` | Active save/load profile version |
| `playerDataJson` | Reserved player payload placeholder |
| `moduleDataEntries` | List of `{ moduleId, payloadJson }` entries |

Unity `JsonUtility` serializes the document. Module data uses list entries instead of a raw dictionary for serializer compatibility.

---

## Save path

| Path | Value |
|------|-------|
| Root | `Application.persistentDataPath/CCS_Survival/Saves/` |
| Slot file | `{slotId}.json` |
| Utility | `CCS_SavePathUtility` |

No encryption or cloud sync in **0.6.0**.

---

## Versioning strategy

| Version field | Usage |
|---------------|-------|
| `version` | Written from `Application.version` on save |
| `profileVersion` | Written from `CCS_SaveLoadProfile.ProfileVersion` |
| Load validation | Rejects saves missing `version`; warns on profile version mismatch |

Future migrations can compare these fields before applying module payloads.

---

## Service registration (0.6.0)

`CCS_SurvivalGameplayServiceRegistration` registers `CCS_SaveLoadService` from `CCS_SaveLoadProfile` on the bootstrap gameplay service host.

`CCS_SaveLoadRuntimeBridge` resolves the service from `CCS_RuntimeHost.ServiceRegistry`.

---

## Events

| Event | When |
|-------|------|
| OnSaveStarted | Save attempt begins |
| OnSaveCompleted | Save file written successfully |
| OnLoadStarted | Load attempt begins |
| OnLoadCompleted | Registered saveables restored |
| OnSaveFailed | Validation or IO failure during save |
| OnLoadFailed | Validation, parse, or restore failure during load |

---

## Profile defaults

Default asset: `Assets/CCS/Survival/Profiles/SaveLoad/CCS_DefaultSaveLoadProfile.asset`

| Setting | Default |
|---------|---------|
| Auto save enabled | false |
| Auto save interval | 300 seconds |
| Max save slots | 10 |

Auto-save execution is deferred until a future milestone.

---

## Development test saveable

`CCS_TestSaveableComponent` stores:

- string value
- integer value
- UTC timestamp captured on save

Registered with `CCS_SaveLoadService` on bootstrap scene startup. Used only to prove registry capture/restore.

Bootstrap setup batch: `CCS.Modules.SaveLoad.Editor.CCS_SaveLoadBootstrapSetup.ExecuteBatch`

---

## Manual debug save/load workflow (0.6.1)

Bootstrap scene includes developer-only controls:

| Component | Role |
|-----------|------|
| `CCS_SaveLoadDebugController` | Manual save/load/delete/list hooks wrapping `CCS_SaveLoadService` |
| `CCS_SaveLoadDebugPanelPresenter` | Minimal HUD panel with status text and manual trigger buttons |

Panel display:

- Selected slot id
- Existing save slots (comma-separated summary)
- Last save/load/delete status (`CCS_SaveLoadResult` message)
- Shortened save root path from `CCS_SavePathUtility.GetShortDisplayPath()`

Manual methods (callable from UI buttons):

- `ManualSaveSelectedSlot()`
- `ManualLoadSelectedSlot()`
- `ManualDeleteSelectedSlot()`
- `RefreshSaveSlotListing()`
- `SelectNextSlot()` / `SelectPreviousSlot()`

Panel anchor: upper-left on `PF_CCS_HUD_Root` canvas — does not block center gameplay view.

### What 0.6.1 proves

- Save/load service initializes on bootstrap
- JSON save files can be created, loaded, listed, and deleted through manual hooks
- Registered development saveables round-trip through save/load
- Debug UI can observe slot state and latest operation result

### Intentionally not saved yet

- World resource node state
- Crafting queues
- Building placed instances (restored at **0.8.4**)
- Combat state
- Wildlife
- Weather
- Quests
- Final player persistence payload (`playerDataJson` remains empty)

---

## Inventory and equipment persistence (0.6.2)

| Saveable | Payload | Version field |
|----------|---------|---------------|
| `CCS_PlayerInventoryService` | `CCS_InventorySaveData` | `saveDataVersion` |
| `CCS_PlayerEquipmentService` | `CCS_EquipmentSaveData` | `saveDataVersion` |

Inventory payload persists slot entries (item definition IDs + quantities), slot count, and captured capacity modifier snapshot fields.

Equipment payload persists equipped slot entries (slot type + item ID + durability), plus aggregate capacity modifier fields.

### Load order dependency

`CCS_SaveableRegistry` restores payloads in `CCS_SaveLoadSaveableIds.ModuleRestoreOrder`:

1. **Inventory first** — item stacks must exist before equipment-dependent gameplay resumes.
2. **Equipment second** — equipped items restore after inventory baseline is applied.
3. **Time of day third** — global clock restores before weather that may read time snapshots.
4. **Weather fourth** — global weather state restores after time baseline is applied.
5. **Shelter fifth** — sheltered state restores before environment recomputes effective values.
6. **Environment sixth** — raw environment simulation restores after weather and shelter baselines are applied.
7. **Building seventh** — registered definition catalog and placed instance records restore after environment.

### Building persistence restore (0.8.4)

| Saveable | Payload | Version field |
|----------|---------|---------------|
| `CCS_BuildingService` | `CCS_BuildingSaveData` | `saveDataVersion` (current: **3**) |

Building payload persists registered piece IDs and placed instance records including world transform, placement order, occupied snap point IDs, and optional target snap metadata.

Restore recreates runtime instances, rebuilds snap points, reapplies occupancy, and spawns primitive cube visuals through `CCS_BuildingInstanceVisualFactory`. Missing definitions or invalid records are skipped with warnings.

Persistence verification harness: `CCS_BuildingPersistenceTestHarness` saves to slot `building_persistence_test`, clears instances, loads, and verifies restored count plus snap occupancy.

Missing item or equipment definitions fail safely during restore (slot skipped, warning logged, no corruption of other slots).

### Debug registration indicators (0.6.2)

Save debug panel displays read-only:

- `Inv Save: Yes/No`
- `Eq Save: Yes/No`

### Persistence test harness

`CCS_InventoryEquipmentPersistenceTestHarness` lives under `Assets/CCS/Modules/Equipment/Runtime/Testing/` (bootstrap only):

1. Waits for harvest/craft pipeline to produce campfire kit
2. Equips test campfire kit equipment
3. Saves to `persistence_test` slot
4. Clears inventory/equipment
5. Loads save and verifies restored quantities and equipped slot

Logs `PASS` or `FAIL` to the console.

Bootstrap setup batch: `CCS.Modules.SaveLoad.Editor.CCS_SaveLoadBootstrapSetup.ExecuteBatch`

---

## Future module integration

1. Implement `CCS_ISaveable` with a stable reverse-DNS `SaveableId`.
2. Register with `CCS_SaveLoadService.RegisterSaveable()` during module startup.
3. Serialize module state to JSON in `CaptureState()`.
4. Deserialize and apply in `RestoreState(string stateJson)`.
5. Unregister on shutdown if the saveable is destroyed.

Deferred gameplay persistence: world resources, combat, wildlife, weather, quests, crafting queues.

---

## Validation

**Editor menu:** **CCS → Survival → Save Load → Validate Save Load**

Validator ID: `ccs.survival.validation.saveload`

Registered via `CCS_SaveLoadValidationRegistration` on the central `CCS_SurvivalValidationPipeline`.

---

## Related docs

- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
- [Inventory Module](../Inventory/Documentation/CCS_Inventory_Module.md)
