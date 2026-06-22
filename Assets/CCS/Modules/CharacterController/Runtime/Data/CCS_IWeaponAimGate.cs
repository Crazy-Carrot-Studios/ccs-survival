// =============================================================================
// SCRIPT: CCS_IWeaponAimGate
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Optional gate for aim locomotion based on weapon ownership state.
// PLACEMENT: Interface. Implemented by CCS_PlayerWeaponLoadout in Weapons module.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Keeps aim strafe disabled until the player owns a weapon.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IWeaponAimGate
    {
        bool CanUseAimMovement { get; }
    }
}
