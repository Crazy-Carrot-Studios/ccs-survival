// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioFitTarget
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Primary Fit Studio target — Holstered Item vs Equipped Item.
// PLACEMENT: Enum used by revamped editor-only Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: User selects fit target first; tool auto-loads pose, profile, and camera.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioFitTarget
    {
        HolsteredItem = 0,
        EquippedItem = 1,
    }
}
