// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioEquippedPoseType
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Equipped pose context for Fit Studio — one-hand revolver vs two-hand future.
// PLACEMENT: Enum used by Fit Studio guidance and pose readout.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.8 defaults to one-hand revolver; two-hand preview is future/experimental.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioEquippedPoseType
    {
        OneHandRevolver = 0,
        TwoHandWeaponPreview = 1,
    }
}
