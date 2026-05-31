using System;

// =============================================================================
// SCRIPT: CCS_InventorySaveSlotEntry
// CATEGORY: Modules / Inventory / Runtime / Data
// PURPOSE: Serializable inventory slot payload for save/load restore.
// PLACEMENT: Stored inside CCS_InventorySaveData slot arrays.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Empty slots use blank itemId and zero quantity.
// =============================================================================

namespace CCS.Modules.Inventory
{
    [Serializable]
    public sealed class CCS_InventorySaveSlotEntry
    {
        #region Variables

        public string itemId = string.Empty;

        public int quantity;

        #endregion
    }
}
