using System;

// =============================================================================
// SCRIPT: CCS_IWeaponCarryStateCameraSource
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Local-player weapon carry state notifications for scene camera switching.
// PLACEMENT: Interface. Implemented by CCS_WeaponCarryStateController in Weapons module.
// AUTHOR: James Schilz
// CREATED: 2026-06-23
// NOTES: Keeps CharacterController free of Weapons assembly references.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IWeaponCarryStateCameraSource
    {
        event Action CarryStateChanged;

        bool ShouldDriveLocalCamera { get; }

        bool WantsFirstPersonAimCamera { get; }

        byte CarryStateValue { get; }
    }
}
