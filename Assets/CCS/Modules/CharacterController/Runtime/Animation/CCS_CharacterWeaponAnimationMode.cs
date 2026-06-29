// =============================================================================
// SCRIPT: CCS_CharacterWeaponAnimationMode
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Future-safe weapon animation presentation mode contract.
// PLACEMENT: Referenced by future presentation bridges. Not wired to gameplay in v0.7.4.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.4 Phase 3C design contract only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    /// <summary>
    /// Target upper-body weapon presentation modes for a future weapon animation layer.
    /// Gameplay weapon ownership remains in the Weapons and AI modules.
    /// </summary>
    public enum CCS_CharacterWeaponAnimationMode
    {
        None = 0,
        SingleRevolver = 1,
        DualRevolver = 2,
    }
}
