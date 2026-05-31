# CCS Survival — Building Module

**Milestone:** 0.8.0 — Building Foundation  
**Module ID:** `ccs.survival.building`  
**Namespace:** `CCS.Modules.Building` (editor: `CCS.Modules.Building.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Architecture foundation complete (definitions and service only; no placement)

---

## Purpose

Provide the **runtime building architecture** that becomes the authoritative system for structure definitions and future placement.

Building owns:

| Concern | 0.8.0 scope |
|---------|-------------|
| Piece definitions | ScriptableObject metadata and categories |
| Definition catalog | Register, lookup, and snapshot known pieces |
| Service persistence | Save/load registered definition IDs |
| Feature flags | Placement, demolition, upgrades disabled |

No placement, snapping, holograms, build mode, durability, repair, demolition, or multiplayer networking in 0.8.0.

---

## Architecture flow

```text
CCS_BuildingPieceDefinition assets
        ↓
CCS_BuildingService (definition catalog)
        ↓
CCS_BuildingProfile startup registration
        ↓
HUD debug count + save/load persistence
        ↓
(Future) placement, snapping, shelter volumes from structures
```

**Critical rule:** Building exposes definitions and catalog state only. It does **not** spawn transforms, mutate inventory, or construct structures in 0.8.0.

---

## Folder layout

```text
Assets/CCS/Modules/Building/
  Runtime/
    Definitions/    → piece types and ScriptableObject definitions
    Data/           → snapshots, state, save payloads
    Services/       → CCS_BuildingService and runtime bridge
    Profiles/       → CCS_BuildingProfile
    Events/         → definition registration and state change events
    Validation/     → runtime validation helpers
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
- Prefab reference placeholder (future spawning)
- Crafting requirements placeholder (future build mode)
- Shelter contribution placeholder (future shelter integration from structures)

---

## Service responsibilities

`CCS_BuildingService`:

- Registers definitions from profile startup list and runtime calls
- Tracks known piece IDs in `CCS_BuildingState`
- Provides lookups and piece snapshots (position/rotation placeholders only)
- Raises `OnBuildingDefinitionRegistered` and `OnBuildingStateChanged`
- Implements `CCS_ISaveable` with ID `ccs.survival.saveable.building.global`

No placement, spawning, construction, or destruction.

---

## Save / load behavior

Restore order (after environment):

**Inventory → Equipment → TimeOfDay → Weather → Shelter → Environment → Building**

Save payload stores registered definition IDs only. No placed structures yet.

---

## Future placement system

Deferred to later milestones:

- Build mode and hologram previews
- Grid snapping and rotation rules
- Prefab spawning at world transforms
- Structure durability, repair, and demolition
- Shelter volume generation from completed structures
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
| Environment Effects | HUD displays registered definition count below shelter lines |
