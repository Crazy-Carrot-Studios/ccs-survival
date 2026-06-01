// =============================================================================
// SCRIPT: CCS_EquippedVisualInstance
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Runtime tracking data for a spawned equipped visual instance.
// PLACEMENT: Owned by CCS_EquipmentVisualController per active socket.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Keeps item ID and equipment slot for save/load and playtest verification.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_EquippedVisualInstance
    {
        #region Variables

        private readonly string itemId;
        private readonly CCS_EquipmentSlotType equipmentSlot;
        private readonly CCS_EquipmentAttachmentSocketType socketType;
        private readonly UnityEngine.GameObject visualRoot;

        #endregion

        public CCS_EquippedVisualInstance(
            string equippedItemId,
            CCS_EquipmentSlotType slot,
            CCS_EquipmentAttachmentSocketType attachmentSocket,
            UnityEngine.GameObject instanceRoot)
        {
            itemId = equippedItemId ?? string.Empty;
            equipmentSlot = slot;
            socketType = attachmentSocket;
            visualRoot = instanceRoot;
        }

        #region Properties

        public string ItemId => itemId;

        public CCS_EquipmentSlotType EquipmentSlot => equipmentSlot;

        public CCS_EquipmentAttachmentSocketType SocketType => socketType;

        public UnityEngine.GameObject VisualRoot => visualRoot;

        public bool IsValid => visualRoot != null;

        #endregion
    }
}
