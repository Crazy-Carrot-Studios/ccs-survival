# CCS Survival — Building Module

**Milestone:** 0.8.1 — Building Placement Foundation  
**Module ID:** `ccs.survival.building`  
**Namespace:** `CCS.Modules.Building` (editor: `CCS.Modules.Building.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Placement foundation complete (build mode, preview, placed instances; no snapping or inventory consumption)

---

## Purpose

Provide the **runtime building architecture** that becomes the authoritative system for structure definitions and placement.

Building owns:

| Concern | 0.8.1 scope |
|---------|-------------|
| Piece definitions | ScriptableObject metadata and categories |
| Definition catalog | Register, lookup, and snapshot known pieces |
| Build mode | Enter/exit placement mode with active definition |
| Placed instances | Track spawned structures with instance IDs |
| Service persistence | Save definition IDs and placed instance records |
| Feature flags | Placement enabled; demolition/upgrades disabled |

No advanced snapping, structural integrity, durability, repair, demolition, inventory consumption, or multiplayer networking in 0.8.1.

---

## Architecture flow

```text
CCS_BuildingPieceDefinition assets
        ↓
CCS_BuildingService (definitions + placed instances)
        ↓
CCS_BuildingPlacementService (build mode + preview + place)
        ↓
CCS_BuildingPlacementPreview (development cube preview)
        ↓
CCS_BuildingPlacementTestHarness (bootstrap verification)
        ↓
HUD debug + save/load placed instance records
        ↓
(Future) snapping, durability, shelter volumes from structures
```

**Critical rule:** Placement does **not** consume inventory resources in 0.8.1.

---

## Folder layout

```text
Assets/CCS/Modules/Building/
  Runtime/
    Definitions/    → piece types and ScriptableObject definitions
    Data/           → instances, placement state/snapshots, save payloads
    Services/       → CCS_BuildingService, CCS_BuildingPlacementService, bridge
    Placement/      → CCS_BuildingPlacementPreview
    Profiles/       → CCS_BuildingProfile
    Events/         → catalog and placement lifecycle events
    Validation/     → runtime validation helpers
    Testing/        → CCS_BuildingPlacementTestHarness
  Editor/
    Validation/     → pipeline validator, menu, bootstrap setup
  Documentation/    → this file

Assets/CCS/Survival/Profiles/Building/
  CCS_DefaultBuildingProfile.asset

Assets/CCS/Survival/Content/Building/Definitions/
  CCS_TestFoundation.asset
  CCS_TestWall.asset
  CCS_TestRoof.asset
```

---

## Piece definitions

`CCS_BuildingPieceType` categories:

Foundation, Floor, Wall, Doorway, Door, WindowWall, Roof, Stair, Pillar, CampStructure, Custom

`CCS_BuildingPieceDefinition` fields:

- Piece ID, display name, description, building piece type
- Prefab reference placeholder (future final art spawning)
- Crafting requirements placeholder (future resource consumption)
- Shelter contribution placeholder (future shelter integration)

---

## Placed instance model

`CCS_BuildingInstance`:

- Instance ID
- Piece ID (definition reference)
- Position and rotation
- Creation time

No durability fields in 0.8.1.

---

## Placement service

`CCS_BuildingPlacementService`:

- `EnterPlacementMode()` / `ExitPlacementMode()`
- `SetActiveDefinition()` / `UpdatePreview()` / `PlaceCurrentPiece()`
- Tracks placement validity through `CCS_BuildingPlacementState`
- Events: `OnPlacementStarted`, `OnPlacementCancelled`, `OnBuildingPlaced`
- Delegates placed instance storage to `CCS_BuildingService`

---

## Preview architecture

`CCS_BuildingPlacementPreview`:

- Development-only cube primitive
- Visible only while placement mode is active and preview is valid
- Foundation, wall, and roof all use cube placeholders
- No hologram shaders or final materials

---

## Save / load behavior

Restore order (after environment):

**Inventory → Equipment → TimeOfDay → Weather → Shelter → Environment → Building**

`CCS_BuildingSaveData` (version 2) stores:

- Registered definition IDs
- Placed instance records (`CCS_BuildingInstanceSaveRecord`)

Full placed instance restore is **deferred** beyond 0.8.1. Capture establishes the serialization model only.

---

## Bootstrap verification

Bootstrap scene includes:

- `CCS_BuildingTestArea` hierarchy root near shelter/resource testing
- `CCS_BuildingPlacementPreview`
- `CCS_BuildingPlacementTestHarness` cycling foundation, wall, and roof every few seconds

---

## Deferred

- Advanced snapping and grid rules
- Structural integrity and support checks
- Durability, repair, and demolition
- Shelter volume generation from placed structures
- Inventory resource consumption during placement
- Multiplayer authority and replication

---

## Validation

**Editor menu:** **CCS → Survival → Building → Validate Building**

Batch entry:

```powershell
Unity.exe -batchmode -nographics -quit `
  -projectPath . `
  -executeMethod CCS.Modules.Building.Editor.CCS_BuildingValidationMenu.ValidateBuilding `
  -logFile Logs/CCS_BuildingValidation.log
```

Bootstrap setup batch:

```powershell
Unity.exe -batchmode -nographics -quit `
  -projectPath . `
  -executeMethod CCS.Modules.Building.Editor.CCS_BuildingBootstrapSetup.ExecuteBatch `
  -logFile Logs/CCS_BuildingBootstrap.log
```

---

## Related modules

| Module | Relationship |
|--------|--------------|
| Crafting | Crafting requirement placeholders on definitions |
| Inventory | Future material consumption during placement |
| Shelter | Future shelter contribution from placed structures |
| Save / Load | Building restores after environment |
| Environment Effects | HUD displays definition count and placement debug lines |
