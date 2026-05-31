using System;

// =============================================================================
// SCRIPT: CCS_InventorySaveData
// CATEGORY: Modules / Inventory / Runtime / Data
// PURPOSE: Root inventory save payload with version and capacity modifier fields.
// PLACEMENT: Serialized by CCS_PlayerInventoryService CaptureState / RestoreState.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: saveDataVersion reserved for future migration. No migration system in 0.6.2.
// =============================================================================

namespace CCS.Modules.Inventory
{
    [Serializable]
    public sealed class CCS_InventorySaveData
    {
        #region Variables

        public const int CurrentSaveDataVersion = 1;

        public int saveDataVersion = CurrentSaveDataVersion;

        public int slotCount;

        public int additionalInventorySlots;

        public float additionalCarryWeight;

        public CCS_InventorySaveSlotEntry[] slots = Array.Empty<CCS_InventorySaveSlotEntry>();

        #endregion
    }
}
