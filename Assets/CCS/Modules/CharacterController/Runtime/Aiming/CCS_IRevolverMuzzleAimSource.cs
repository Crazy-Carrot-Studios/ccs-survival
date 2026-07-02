using UnityEngine;

// =============================================================================
// SCRIPT: CCS_IRevolverMuzzleAimSource
// CATEGORY: Modules / CharacterController / Runtime / Aiming
// PURPOSE: Read-only muzzle line-of-sight and convergence contract for presentation.
// PLACEMENT: Implemented by CCS_RevolverMuzzleLineOfSightResolver (planned v0.7.14+).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Interface-only in v0.7.11. Does not change gameplay damage or hitscan authority.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IRevolverMuzzleAimSource
    {
        bool HasValidMuzzlePoint { get; }

        Vector3 MuzzleWorldPoint { get; }

        Vector3 MuzzleForward { get; }

        bool HasClearLineToAimTarget { get; }

        Vector3 ConvergenceWorldPoint { get; }

        bool HasValidConvergence { get; }
    }
}
