# CCS Survival — Save / Load Module

**Milestone:** 0.6.0 — Save / Load Foundation  
**Module ID:** `ccs.survival.saveload`  
**Namespace:** `CCS.Modules.SaveLoad` (editor: `CCS.Modules.SaveLoad.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Persistence framework complete at **0.6.0**. Gameplay module saves deferred.

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
    Testing/        → CCS_TestSaveableComponent (development only)
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
| `RestoreAllModuleStates` | Applies payloads to registered saveables during load |

Future module examples: Inventory, Equipment, Building, World Resources, Weather.

Each saveable owns its JSON payload format. The service stores payloads keyed by `SaveableId`.

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

## Future module integration

1. Implement `CCS_ISaveable` with a stable reverse-DNS `SaveableId`.
2. Register with `CCS_SaveLoadService.RegisterSaveable()` during module startup.
3. Serialize module state to JSON in `CaptureState()`.
4. Deserialize and apply in `RestoreState(string stateJson)`.
5. Unregister on shutdown if the saveable is destroyed.

Deferred gameplay persistence: world resources, building pieces, combat, wildlife, weather, quests.

---

## Validation

**Editor menu:** **CCS → Survival → Save Load → Validate Save Load**

Validator ID: `ccs.survival.validation.saveload`

Registered via `CCS_SaveLoadValidationRegistration` on the central `CCS_SurvivalValidationPipeline`.

---

## Related docs

- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
- [Inventory Module](../Inventory/Documentation/CCS_Inventory_Module.md)
