# CCS Survival — Building Module

**Milestone:** 0.8.5 — Building Shelter Integration  
**Module ID:** `ccs.survival.building`  
**Namespace:** `CCS.Modules.Building` (editor: `CCS.Modules.Building.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Placed instances restore from save and contribute shelter protection through the Shelter service.

---

## Shelter integration (0.8.5)

Building pieces with `contributesToShelter` publish `CCS_BuildingShelterContribution` records. `CCS_ShelterService` applies the **maximum** protection values from contributions whose coverage radius contains the subject position. Environment Effects reads shelter output only.

---

## Purpose

Provide the **runtime building architecture** for structure definitions, inventory-backed placement, basic snap alignment, and **persistence restore** of placed instances.

Building owns:

| Concern | 0.8.4 scope |
|---------|-------------|
| Piece definitions | Metadata, build costs, and authored snap points |
| Snap matching | Compatible snap search and preview alignment |
| Inventory costs | Validate and consume resources before placement |
| Placed instances | Track structures with runtime snap point occupancy |
| Service persistence | Save and restore definition IDs and placed instance records |
| Visual restore | Cube placeholders via shared visual factory |
| Feature flags | Placement enabled; demolition/upgrades disabled |

No advanced grid snapping, structural integrity, durability, repair, demolition, or multiplayer networking in 0.8.4.

---

## Save payload (`CCS_BuildingSaveData` v3)

| Field | Purpose |
|-------|---------|
| `saveDataVersion` | Format version (current: **3**) |
| `registeredPieceIds` | Catalog of registered definition IDs |
| `placedInstanceRecords` | Serializable placed structures |

Each `CCS_BuildingInstanceSaveRecord` persists:

- `instanceId` — stable logical ID (not Unity instance ID)
- `pieceId` — definition ID
- World position and rotation (`positionX/Y/Z`, `rotationX/Y/Z/W`)
- `creationTime`
- `placedOrderIndex` — restore order
- `occupiedSnapPointIds` — snap points marked occupied on this instance
- `targetSnapInstanceId` / `targetSnapPointId` — optional parent snap metadata

Version 2 saves restore with default order index and empty occupancy metadata.

---

## Restore flow

```text
CCS_SaveLoadService load
        ↓
CCS_BuildingService.RestoreState(json)
        ↓
Clear runtime placed instances + destroy visuals
        ↓
Restore registered definition catalog
        ↓
CCS_BuildingDefinitionLookup.TryResolveDefinition(pieceId)
        ↓
Recreate CCS_BuildingInstance (sorted by placedOrderIndex)
        ↓
InitializeRuntimeSnapPoints + ApplyOccupiedSnapPoints
        ↓
CCS_BuildingInstanceVisualFactory.SpawnInstanceVisual()
        ↓
OnBuildingStateChanged (SavedBuildingRecordCount / RestoredBuildingCount updated)
```

Invalid records are skipped with warning logs. Missing definitions fail safely without corrupting valid instances.

---

## Definition lookup

`CCS_BuildingDefinitionLookup` resolves `pieceId` → `CCS_BuildingPieceDefinition` using:

1. Profile `StartupDefinitions`
2. Registered runtime catalog entries

Used during restore only; placement continues to use `CCS_BuildingService.TryGetDefinition`.

---

## Visual restore

`CCS_BuildingInstanceVisualFactory` spawns **cube placeholders** for Foundation, Wall, and Roof when prefabs are not assigned.

- Shared by placement (`TryAddPlacedInstance`) and restore
- Parents under `CCS_BuildingTestArea` when present
- Creates `CCS_BuildingRuntimeVisualRoot` at runtime when test area is missing
- No final art or prefab requirement yet

Final prefab spawning, hologram previews, and art-driven visuals are deferred.

---

## Snap point architecture

```text
CCS_BuildingSnapPoint (definition authoring)
        ↓
CCS_BuildingInstance.InitializeRuntimeSnapPoints()
        ↓
CCS_BuildingRuntimeSnapPoint (world-space + occupied flag)
        ↓
CCS_BuildingSnapCompatibilityUtility (explicit rules)
        ↓
CCS_BuildingPlacementService.FindBestSnapMatch()
        ↓
UpdatePreviewWithSnap() → PlaceCurrentPieceUsingSnap()
```

**Snap occupancy persistence:** occupied snap IDs are captured per instance on save and reapplied on restore so future wall/roof placement cannot reuse occupied points.

---

## Compatibility rules

| Target snap | Accepts source |
|-------------|----------------|
| `FoundationEdge` | `WallBottom` |
| `WallTop` | `RoofEdge` |
| `Free` | `Free` |

---

## Test definitions (bootstrap)

| Piece | Snap points |
|-------|-------------|
| Foundation | `foundation_edge_top` (`FoundationEdge`) |
| Wall | `wall_bottom` (`WallBottom`), `wall_top` (`WallTop`) |
| Roof | `roof_edge` (`RoofEdge`) |

Harness sequence: foundation free-place → wall snap to foundation → roof snap to wall.

---

## Persistence test harness

`CCS_BuildingPersistenceTestHarness` (bootstrap only):

1. Waits for placement harness to place foundation, wall, and roof
2. Saves to slot `building_persistence_test`
3. Clears placed instances
4. Loads slot
5. Verifies restored count and snap occupancy (`foundation_edge_top`, `wall_top`)
6. Logs `PASS` or `FAIL`

---

## HUD debug display

Environment panel shows:

- Building definition count
- Placement active / selected piece / placed count
- Snap target and placement validity
- **Saved Building Records:** count from last restore payload
- **Restored Buildings:** count of successfully recreated instances

---

## Deferred

- Advanced grid snapping and rotation rules
- Structural integrity and support validation
- Durability, repair, and demolition
- Final prefab spawning and hologram previews
- Multiplayer authority and replication

---

## Related documentation

- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
- [Save/Load Module](../../SaveLoad/Documentation/CCS_Save_Load_Module.md)
- [Inventory Module](../../Inventory/Documentation/CCS_Inventory_Module.md)
