using UnityEngine;

// =============================================================================
// SCRIPT: CCS_IRevolverAimTargetSource
// CATEGORY: Modules / CharacterController / Runtime / Aiming
// PURPOSE: Read-only camera/mouse aim target contract for presentation systems.
// PLACEMENT: Implemented by CCS_RevolverAimTargetResolver (v0.7.12+).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Does not drive gameplay fire, damage, ammo, or ownership.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IRevolverAimTargetSource
    {
        bool HasValidAimTarget { get; }

        Vector3 AimWorldPoint { get; }

        Vector3 AimDirection { get; }

        float AimDistance { get; }

        bool IsObstructed { get; }
    }
}
