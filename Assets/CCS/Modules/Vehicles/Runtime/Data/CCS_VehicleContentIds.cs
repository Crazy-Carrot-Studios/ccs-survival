// =============================================================================
// SCRIPT: CCS_VehicleContentIds
// CATEGORY: Modules / Vehicles / Runtime / Data
// PURPOSE: Stable content identifiers for wagon foundation validation and wiring.
// PLACEMENT: Referenced by bootstrap, validators, and composition binds.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public static class CCS_VehicleContentIds
    {
        public const string FrontierWagonVehicleId = "ccs.survival.vehicle.wagon.frontier";
        public const string FrontierWagonItemId = "ccs.survival.item.vehicle.frontierwagon";
        public const string WagonCargoContainerId = "ccs.survival.storage.wagon.cargo";
        public const string WagonPrefabName = "PF_CCS_FrontierWagon";
        public const string HitchCompatibleHorseMountId = "ccs.survival.mount.horse";
    }
}
