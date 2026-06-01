# CCS Survival — Save System Module

**Milestone:** 1.0.1 — Death, Respawn & Save Foundation  
**Module ID:** `ccs.survival.savesystem`  
**Namespace:** `CCS.Modules.SaveSystem` (editor: `CCS.Modules.SaveSystem.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-06-01  

---

## Purpose

Provide a **unified JSON save file** for the core survival loop:

- Player transform  
- Hunger, thirst, stamina  
- Inventory slots  
- Gathering node depletion / respawn timers  
- Campfire cooking station state  
- Placed building instances  

File path: `Application.persistentDataPath/CCS_Survival_Save.json`

---

## Key types

| Type | Role |
|------|------|
| `CCS_SaveProfile` | Save file name, auto-save interval, debug logging |
| `CCS_SaveData` | Root serializable payload (`saveVersion`, `savedAtUtc`, nested sections) |
| `CCS_SaveService` | `SaveGame`, `LoadGame`, `DeleteSave`, `HasSave` |
| `CCS_SaveRuntimeBridge` | Resolves service from `CCS_RuntimeHost` |
| `CCS_SaveStartupLoader` | Loads save on play or applies starter loadout |
| `CCS_SaveDebugController` | F5 save, F9 load, F8 delete (development) |

---

## Events

| Event | When |
|-------|------|
| `SaveStarted` / `SaveCompleted` | Unified save write lifecycle |
| `LoadStarted` / `LoadCompleted` | Unified save read lifecycle |

---

## Composition

`CCS_SurvivalGameplayServiceRegistration` registers `CCS_SaveService` and binds inventory, survival core, gathering, and building services.  
`CCS_SaveStartupLoader` completes player transform binding and runs `TryLoadOnStartup()` before starter loadout.

Legacy multi-slot `CCS_SaveLoadService` remains for module-level `CCS_ISaveable` payloads.

---

## Bootstrap

Batch: `CCS.Modules.SaveSystem.Editor.CCS_SaveBootstrapSetup.ExecuteBatch`

Creates default profile, wires gameplay host, adds startup loader / debug controller, respawn point, and gathering/campfire save ids.

---

## Validation

Validator ID: `ccs.survival.validation.savesystem`
