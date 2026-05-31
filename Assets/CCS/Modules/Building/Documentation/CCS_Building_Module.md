# CCS Survival — Building Module

**Milestone:** 0.8.2 — Building Construction Costs & Placement Validation  
**Module ID:** `ccs.survival.building`  
**Namespace:** `CCS.Modules.Building` (editor: `CCS.Modules.Building.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Placement with inventory build costs complete (validation, consumption, rollback, HUD notifications; no snapping)

---

## Purpose

Provide the **runtime building architecture** that becomes the authoritative system for structure definitions and placement.

Building owns:

| Concern | 0.8.2 scope |
|---------|-------------|
| Piece definitions | ScriptableObject metadata, categories, and build costs |
| Definition catalog | Register, lookup, and snapshot known pieces |
| Build mode | Enter/exit placement mode with active definition |
| Inventory costs | Validate and consume resources before placement |
| Placed instances | Track spawned structures with instance IDs |
| Service persistence | Save definition IDs and placed instance records |
| Feature flags | Placement enabled; demolition/upgrades disabled |

No advanced snapping, structural integrity, durability, repair, demolition, or multiplayer networking in 0.8.2.

---

## Architecture flow

```text
CCS_BuildingPieceDefinition assets (+ BuildCostEntries)
        ↓
CCS_BuildingService (definitions + placed instances)
        ↓
CCS_PlayerInventoryService (HasItem / RemoveItem / AddItem)
        ↓
CCS_BuildingPlacementValidationUtility (validate + consume + rollback)
        ↓
CCS_BuildingPlacementService.TryPlaceCurrentPiece()
        ↓
CCS_BuildingPlacementPreview (development cube preview)
        ↓
CCS_BuildingPlacementTestHarness (bootstrap verification + resource seeding)
        ↓
HUD notifications + save/load placed instance records
        ↓
(Future) snapping, durability, shelter volumes from structures
```

**Critical rule:** Placement validates inventory first. Costs are consumed only after validation passes. Partial consumption rolls back with no item loss.

---

## Folder layout

```text
Assets/CCS/Modules/Building/
  Runtime/
    Definitions/    → piece types, definitions, build cost entries
    Data/           → instances, placement state/snapshots, save payloads
    Services/       → CCS_BuildingService, CCS_BuildingPlacementService, bridge
    Placement/      → CCS_BuildingPlacementPreview
    Profiles/       → CCS_BuildingProfile
    Events/         → catalog and placement lifecycle events
    Validation/     → placement validation + runtime helpers
    Testing/        → CCS_BuildingPlacementTestHarness
  Editor/
    Validation/     → pipeline validator, menu, bootstrap setup
  Documentation/    → this file

Assets/CCS/Survival/Profiles/Building/
  CCS_DefaultBuildingProfile.asset

Assets/CCS/Survival/Content/Building/Definitions/
  CCS_TestFoundation.asset   (Wood x4, Stone x2)
  CCS_TestWall.asset         (Wood x6)
  CCS_TestRoof.asset         (Wood x4, Fiber x3)
```

---

## Piece definitions

`CCS_BuildingPieceType` categories:

Foundation, Floor, Wall, Doorway, Door, WindowWall, Roof, Stair, Pillar, CampStructure, Custom

`CCS_BuildingPieceDefinition` fields:

- Piece ID, display name, description, building piece type
- `BuildCostEntries` — list of `CCS_BuildingCostEntry` (item definition + quantity)
- Prefab reference placeholder (future final art spawning)
- Legacy crafting requirements placeholder
- Shelter contribution placeholder (future shelter integration)

`CCS_BuildingCostEntry`:

- `ItemDefinition` — existing inventory item (Wood, Stone, Fiber test items)
- `Quantity` — amount consumed per successful placement

---

## Placed instance model

`CCS_BuildingInstance`:

- Instance ID
- Piece ID (definition reference)
- Position and rotation
- Creation time

No durability fields in 0.8.2.

---

## Placement service

`CCS_BuildingPlacementService`:

- `EnterPlacementMode()` / `ExitPlacementMode()`
- `SetActiveDefinition()` / `UpdatePreview()` / `TryPlaceCurrentPiece()` / `PlaceCurrentPiece()`
- `BindInventoryService()` — required for cost validation and consumption
- Tracks placement validity through `CCS_BuildingPlacementState`
- Events: `PlacementStarted`, `PlacementCancelled`, `BuildingPlaced`, `PlacementFailed`
- Delegates placed instance storage to `CCS_BuildingService`

`TryPlaceCurrentPiece()` flow:

1. Validate service, definition, preview, and inventory costs
2. Consume all build costs (rollback on partial failure)
3. Register placed instance (restore costs if registration fails)
4. Raise `BuildingPlaced` on success or `PlacementFailed` on failure

---

## Preview architecture

`CCS_BuildingPlacementPreview`:

- Development-only cube primitive
- Visible only while placement mode is active and preview is valid
- Foundation, wall, and roof all use cube placeholders
- No hologram shaders or final materials

---

## HUD notifications

Via existing `CCS_HudPresentationService` notification pipeline:

| Outcome | Message |
|---------|---------|
| Success | `Placed Foundation`, `Placed Wall`, `Placed Roof` |
| Missing item | `Missing Wood`, `Missing Stone`, `Missing Fiber` |

---

## Save / load behavior

Restore order (after environment):

**Inventory → Equipment → TimeOfDay → Weather → Shelter → Environment → Building**

`CCS_BuildingSaveData` (version 2) stores:

- Registered definition IDs
- Placed instance records (`CCS_BuildingInstanceSaveRecord`)

Full placed instance restore is **deferred** beyond 0.8.2. Capture establishes the serialization model only. No save format changes beyond cost data references on definitions.

---

## Bootstrap verification

Bootstrap scene includes:

- `CCS_BuildingTestArea` hierarchy root near shelter/resource testing
- `CCS_BuildingPlacementPreview`
- `CCS_BuildingPlacementTestHarness` cycling foundation, wall, and roof every 4 seconds
- Harness seeds Wood/Stone/Fiber into inventory before automated placement

---

## Deferred

- Advanced snapping and grid rules
- Structural integrity and support checks
- Durability, repair, and demolition
- Final prefab spawning and hologram previews
- Full placed instance restore from save
- Multiplayer authority and replication

---

## Validation

**Editor menu:** CCS → Survival → Building → Validate Building

**Batch:**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\james\OneDrive\Documents\GitHub\ccs-survival" `
  -executeMethod CCS.Modules.Building.Editor.CCS_BuildingValidationMenu.RunBuildingValidation `
  -logFile Logs/CCS_BuildingValidation.log
```

**Bootstrap setup:**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\james\OneDrive\Documents\GitHub\ccs-survival" `
  -executeMethod CCS.Modules.Building.Editor.CCS_BuildingBootstrapSetup.ExecuteBatch `
  -logFile Logs/CCS_BuildingBootstrap.log
```

---

## Related documentation

- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
- [Inventory Module](../../Inventory/Documentation/CCS_Inventory_Module.md)
- [Modules README](../../README.md)
