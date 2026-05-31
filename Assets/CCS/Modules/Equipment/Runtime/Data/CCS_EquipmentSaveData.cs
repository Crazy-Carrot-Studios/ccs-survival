using System;

// =============================================================================
// SCRIPT: CCS_EquipmentSaveData
// CATEGORY: Modules / Equipment / Runtime / Data
// PURPOSE: Root equipment save payload with version and capacity modifier fields.
// PLACEMENT: Serialized by CCS_PlayerEquipmentService CaptureState / RestoreState.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: saveDataVersion reserved for future migration. Restores after inventory load.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [Serializable]
    public sealed class CCS_EquipmentSaveData
    {
        #region Variables

        public const int CurrentSaveDataVersion = 1;

        public int saveDataVersion = CurrentSaveDataVersion;

        public int additionalInventorySlots;

        public float additionalCarryWeight;

        public CCS_EquipmentSaveSlotEntry[] equippedSlots = Array.Empty<CCS_EquipmentSaveSlotEntry>();

        #endregion
    }
}
