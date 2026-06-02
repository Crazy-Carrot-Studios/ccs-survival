// =============================================================================
// SCRIPT: CCS_MountContentIds
// CATEGORY: Modules / Mounts / Runtime / Data
// PURPOSE: Stable content identifiers for mount foundation validation and wiring.
// PLACEMENT: Referenced by bootstrap, validators, and composition binds.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public static class CCS_MountContentIds
    {
        public const string HorseMountId = "ccs.survival.mount.horse";
        public const string FrontierHorseItemId = "ccs.survival.item.mount.frontierhorse";
        public const string FrontierStableVendorId = "ccs.survival.vendor.frontier.stable";
        public const string HorseSaddlebagContainerId = "ccs.survival.storage.horse.saddlebag";
        public const string HorsePrefabName = "PF_CCS_Horse";
    }
}
