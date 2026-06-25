// =============================================================================
// SCRIPT: CCS_RevolverAimPhase
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Canonical revolver upper-body aim phases for layer weight and reticle gating.
// PLACEMENT: Referenced by CCS_RevolverUpperBodyAnimator and weapons reticle drivers.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.15 — NoAim → Drawing → FullDraw → Returning → NoAim.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_RevolverAimPhase
    {
        NoAim = 0,
        Drawing = 1,
        FullDraw = 2,
        Returning = 3,
    }
}
