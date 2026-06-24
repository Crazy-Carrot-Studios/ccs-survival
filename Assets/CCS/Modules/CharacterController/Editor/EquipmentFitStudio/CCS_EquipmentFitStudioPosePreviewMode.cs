// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPosePreviewMode
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor-only pose preview modes for Equipment Fit Studio tuning.
// PLACEMENT: Enum used by pose preview utility and Fit Studio window state.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Pose preview does not save animation or bone transforms to profiles.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioPosePreviewMode
    {
        Neutral = 0,
        RevolverAim = 1,
        RevolverFireFrame = 2,
    }
}
