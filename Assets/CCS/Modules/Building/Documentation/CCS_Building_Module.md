# CCS Survival — Building Module

**Milestone:** 0.8.3 — Building Snapping Foundation  
**Module ID:** `ccs.survival.building`  
**Namespace:** `CCS.Modules.Building` (editor: `CCS.Modules.Building.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Basic snap matching complete (free foundation placement, wall/roof required snapping, occupancy; no structural integrity)

---

## Purpose

Provide the **runtime building architecture** for structure definitions, inventory-backed placement, and basic snap alignment.

Building owns:

| Concern | 0.8.3 scope |
|---------|-------------|
| Piece definitions | Metadata, build costs, and authored snap points |
| Snap matching | Compatible snap search and preview alignment |
| Inventory costs | Validate and consume resources before placement |
| Placed instances | Track structures with runtime snap point occupancy |
| Service persistence | Save definition IDs and placed instance records |
| Feature flags | Placement enabled; demolition/upgrades disabled |

No advanced grid snapping, structural integrity, durability, repair, demolition, or multiplayer networking in 0.8.3.

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

**Snap point types:** `FoundationEdge`, `WallBottom`, `WallTop`, `RoofEdge`, `Free`

**Definition fields:**

- `SnapPoints` — authored local snap points per piece
- `AllowsFreePlacement` — foundation may free-place
- `RequiresSnapPoint` — wall and roof must snap before placement

**Runtime fields (`CCS_BuildingRuntimeSnapPoint`):**

- Instance ID, snap point ID, type
- World position and rotation (updated from instance transform)
- Occupied flag (set when another piece snaps to this point)

---

## Compatibility rules

| Target snap | Accepts source |
|-------------|----------------|
| `FoundationEdge` | `WallBottom` |
| `WallTop` | `RoofEdge` |
| `Free` | `Free` |

Rules are explicit in `CCS_BuildingSnapCompatibilityUtility`. No angle validation or structural support checks yet.

---

## Free placement vs required snapping

| Piece | Allows free | Requires snap |
|-------|-------------|---------------|
| Foundation | Yes | No |
| Wall | No | Yes (to foundation edge) |
| Roof | No | Yes (to wall top) |

If a required snap is missing or occupied, preview becomes invalid and placement fails safely without consuming inventory.

---

## Placement flow

1. `UpdatePreviewWithSnap(hintPosition)` searches nearby placed instances for compatible unoccupied snap pairs
2. Required pieces fail preview when no valid snap exists
3. `PlaceCurrentPieceUsingSnap()` validates inventory, consumes costs, registers instance, and marks target snap occupied
4. Partial consumption or registration failure rolls back inventory costs

---

## Test definitions (bootstrap)

| Piece | Snap points |
|-------|-------------|
| Foundation | `foundation_edge_top` (`FoundationEdge`) |
| Wall | `wall_bottom` (`WallBottom`), `wall_top` (`WallTop`) |
| Roof | `roof_edge` (`RoofEdge`) |

Harness sequence: foundation free-place → wall snap to foundation → roof snap to wall.

---

## HUD debug display

Environment panel shows:

- Building definition count
- Placement active / selected piece / placed count
- **Snap Target:** `None` or snap type name
- **Placement Valid:** `Yes` / `No`

---

## Deferred

- Advanced grid snapping and rotation rules
- Structural integrity and support validation
- Durability, repair, and demolition
- Final prefab spawning and hologram previews
- Full placed instance restore from save
- Multiplayer authority and replication

---

## Related documentation

- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
- [Inventory Module](../../Inventory/Documentation/CCS_Inventory_Module.md)
