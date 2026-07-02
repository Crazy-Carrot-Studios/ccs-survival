// =============================================================================
// SCRIPT: CCS_RevolverReticleMode
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Planned reticle convergence display modes for single and dual revolver aim.
// PLACEMENT: Used by CCS_RevolverReticleConvergenceProfile (planned v0.7.15+).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Enum-only in v0.7.11. Dual modes must not activate for single revolver without profile.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_RevolverReticleMode
    {
        SingleCameraIntent = 0,
        SingleMuzzleConvergence = 1,
        HybridIntentAndMuzzle = 2,
        DualMuzzleReticles = 3,
    }
}
