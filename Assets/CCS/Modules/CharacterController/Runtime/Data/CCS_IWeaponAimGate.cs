// =============================================================================
// SCRIPT: CCS_IWeaponAimGate
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Optional gate for combat locomotion and firearm aim camera routing.
// PLACEMENT: Interface. Implemented by CCS_WeaponCarryStateController in Weapons module.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.9 — combat locomotion when weapon is in hands or aiming; FP aim camera only while aiming.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IWeaponAimGate
    {
        bool CanUseAimMovement { get; }

        bool CanUseFirearmAimCamera { get; }
    }
}
