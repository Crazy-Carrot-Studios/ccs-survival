// =============================================================================
// SCRIPT: CCS_VehicleState
// CATEGORY: Modules / Vehicles / Runtime / Data
// PURPOSE: Runtime state for owned frontier vehicles (wagon foundation).
// PLACEMENT: Used by CCS_VehicleService and CCS_VehicleSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public enum CCS_VehicleState
    {
        Idle = 0,
        Hitched = 1,
        Moving = 2,
        Parked = 3,
        Stored = 4
    }
}
