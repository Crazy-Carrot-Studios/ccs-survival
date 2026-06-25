// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWeaponForwardAxis
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Configurable weapon barrel/forward axis for Fit Studio roll controls.
// PLACEMENT: Editor-only enum used by weapon rotation utilities.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: Roll / Side Tilt rotates around the selected forward axis.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioWeaponForwardAxis
    {
        LocalPositiveZ = 0,
        LocalNegativeZ = 1,
        LocalPositiveX = 2,
        LocalNegativeX = 3,
    }
}
