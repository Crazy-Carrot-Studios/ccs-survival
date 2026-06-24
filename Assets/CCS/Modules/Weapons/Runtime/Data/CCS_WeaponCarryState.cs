// =============================================================================
// SCRIPT: CCS_WeaponCarryState
// CATEGORY: Modules / Weapons / Runtime / Data
// PURPOSE: Lightweight weapon carry state for locomotion, visuals, and camera routing.
// PLACEMENT: Runtime data enum. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-23
// NOTES: v0.6.9 revolver flow — Holstered default after pickup, Aiming while RMB held.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public enum CCS_WeaponCarryState : byte
    {
        None = 0,
        Holstered = 1,
        EquippedInHands = 2,
        Aiming = 3
    }
}
