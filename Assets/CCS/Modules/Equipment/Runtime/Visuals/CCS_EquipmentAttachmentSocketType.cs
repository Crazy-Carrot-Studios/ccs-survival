// =============================================================================
// SCRIPT: CCS_EquipmentAttachmentSocketType
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Named transform sockets for visible equipped item attachment on the player rig.
// PLACEMENT: Used by CCS_EquipmentAttachmentSocket and CCS_EquipmentVisualDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Transform-based sockets for placeholder visuals. Bone sockets planned for later.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public enum CCS_EquipmentAttachmentSocketType
    {
        RightHand = 0,
        LeftHand = 1,
        Back = 2,
        LeftHip = 3,
        RightHip = 4,
        Chest = 5,
        Head = 6,
        Backpack = 7
    }
}
