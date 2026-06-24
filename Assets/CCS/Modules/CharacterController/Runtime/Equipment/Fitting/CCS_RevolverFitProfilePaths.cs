// =============================================================================
// SCRIPT: CCS_RevolverFitProfilePaths
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Fitting
// PURPOSE: Runtime-safe revolver fit profile asset paths for readback validation.
// PLACEMENT: Constants only. Used by editor play mode reload and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Paths must match EquipmentFitting profile assets on disk.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_RevolverFitProfilePaths
    {
        public const string RightHipHolsterFitPath =
            CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath;

        public const string RightHandEquippedFitPath =
            CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath;
    }
}
