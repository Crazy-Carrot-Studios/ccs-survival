using System;

// =============================================================================
// SCRIPT: CCS_IRevolverAnimationState
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Read-only revolver animation state/events for upper-body animator bridge.
// PLACEMENT: Interface. Implemented by CCS_RevolverController in Weapons module.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Keeps CharacterController animation bridge decoupled from weapon gameplay logic.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IRevolverAnimationState
    {
        bool IsRevolverOwned { get; }

        bool RevolverAimHeld { get; }

        bool RevolverIsReloading { get; }

        event Action RevolverFired;

        event Action RevolverReloadStarted;

        event Action RevolverReloadCompleted;
    }
}
