// =============================================================================
// SCRIPT: CCS_AimReticleMode
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Reticle positioning modes for Master Test aim visuals.
// PLACEMENT: Enum used by CCS_MuzzleDrivenReticleController.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Hybrid mode is default — camera center with clamped muzzle drift.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public enum CCS_AimReticleMode
    {
        CenterLocked = 0,
        HybridCameraCenterWithMuzzleDrift = 1,
        RawMuzzleProjection = 2
    }
}
