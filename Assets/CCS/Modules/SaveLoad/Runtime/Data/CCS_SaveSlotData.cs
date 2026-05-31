// =============================================================================
// SCRIPT: CCS_SaveSlotData
// CATEGORY: Modules / SaveLoad / Runtime / Data
// PURPOSE: Describes one on-disk save slot for listing and selection.
// PLACEMENT: Returned by CCS_SaveLoadService.EnumerateSaveSlots.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: File path is runtime-only and not serialized into save JSON.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public sealed class CCS_SaveSlotData
    {
        #region Public Methods

        public CCS_SaveSlotData(string slotId, CCS_SaveMetadata metadata, string filePath)
        {
            SlotId = slotId ?? string.Empty;
            Metadata = metadata ?? new CCS_SaveMetadata();
            FilePath = filePath ?? string.Empty;
        }

        #endregion

        #region Properties

        public string SlotId { get; }

        public CCS_SaveMetadata Metadata { get; }

        public string FilePath { get; }

        #endregion
    }
}
