// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioFitMode
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Fit Studio operating mode — Edit Fit Preview vs Play Mode Runtime Test.
// PLACEMENT: Enum used by Fit Studio window and preview player utility.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Edit mode fitting uses editor preview player; Play Mode verifies saved profiles.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioFitMode
    {
        EditFitPreview = 0,
        PlayModeRuntimeTest = 1,
        PlayModeAimFit = 2,
    }
}
