using System;

// =============================================================================
// SCRIPT: CCS_EquipmentSaveSlotEntry
// CATEGORY: Modules / Equipment / Runtime / Data
// PURPOSE: Serializable equipped slot payload for save/load restore.
// PLACEMENT: Stored inside CCS_EquipmentSaveData equipped slot arrays.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: itemId references the linked inventory item definition identity.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [Serializable]
    public sealed class CCS_EquipmentSaveSlotEntry
    {
        #region Variables

        public string slotType = string.Empty;

        public string itemId = string.Empty;

        public bool hasDurability;

        public float currentDurability;

        public float maxDurability;

        #endregion
    }
}
