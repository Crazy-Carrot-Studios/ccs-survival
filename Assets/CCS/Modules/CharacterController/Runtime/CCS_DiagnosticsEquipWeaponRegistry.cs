// =============================================================================
// SCRIPT: CCS_DiagnosticsEquipWeaponRegistry
// CATEGORY: Modules / CharacterController / Runtime
// PURPOSE: Validation-scene registry for visual revolver equip/holster diagnostics.
// PLACEMENT: Runtime static registry. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Diagnostics manager syncs EquipWeapon on validation scenes only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_DiagnosticsEquipWeaponRegistry
    {
        public static bool EquipWeapon { get; set; } = true;
    }
}
