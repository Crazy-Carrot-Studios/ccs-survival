using System;

// =============================================================================
// SCRIPT: CCS_SaveMetadata
// CATEGORY: Modules / SaveLoad / Runtime / Data
// PURPOSE: Lightweight metadata describing a save slot without full module payloads.
// PLACEMENT: Returned by save slot enumeration and embedded in CCS_SaveSlotData.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Version fields support future migration checks.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    [Serializable]
    public sealed class CCS_SaveMetadata
    {
        #region Variables

        public string saveId = string.Empty;

        public string slotId = string.Empty;

        public string timestampUtc = string.Empty;

        public string version = string.Empty;

        public string profileVersion = string.Empty;

        #endregion

        #region Public Methods

        public static CCS_SaveMetadata FromSaveGameData(CCS_SaveGameData saveGameData, string resolvedSlotId)
        {
            if (saveGameData == null)
            {
                return new CCS_SaveMetadata { slotId = resolvedSlotId ?? string.Empty };
            }

            return new CCS_SaveMetadata
            {
                saveId = saveGameData.SaveId ?? string.Empty,
                slotId = string.IsNullOrWhiteSpace(saveGameData.SlotId)
                    ? resolvedSlotId ?? string.Empty
                    : saveGameData.SlotId,
                timestampUtc = saveGameData.TimestampUtc ?? string.Empty,
                version = saveGameData.Version ?? string.Empty,
                profileVersion = saveGameData.ProfileVersion ?? string.Empty
            };
        }

        #endregion
    }
}
