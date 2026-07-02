using UnityEngine;

// =============================================================================
// SCRIPT: CCS_IRevolverAimPresentationStateSource
// CATEGORY: Modules / CharacterController / Runtime / Aiming
// PURPOSE: Aggregate read-only aim presentation state for body, arm IK, muzzle, and reticle.
// PLACEMENT: Implemented by CCS_RevolverAimPresentationState (planned v0.7.12+).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Interface-only in v0.7.11. Presentation-only; local owner scope expected at implementation.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IRevolverAimPresentationStateSource :
        CCS_IRevolverAimTargetSource,
        CCS_IRevolverMuzzleAimSource
    {
        bool IsAimIntentActive { get; }

        bool IsReticleVisible { get; }

        bool ShouldHideReticleImmediately { get; }

        Vector2 ConvergenceScreenPoint { get; }

        float BodyYawBiasDegrees { get; }

        float BodyPitchBiasDegrees { get; }

        float RightHandIkWeight { get; }
    }
}
