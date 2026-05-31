// =============================================================================
// SCRIPT: CCS_SaveLoadDebugState
// CATEGORY: Modules / SaveLoad / Runtime / Testing
// PURPOSE: Read-only snapshot for save/load debug panel display.
// PLACEMENT: Produced by CCS_SaveLoadDebugController for UI presenters.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Development-only display data. Includes gameplay save registration indicators.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public sealed class CCS_SaveLoadDebugState
    {
        #region Public Methods

        public CCS_SaveLoadDebugState(
            string selectedSlotId,
            string listedSaveSlotsSummary,
            string lastOperationSummary,
            string shortenedSavePath,
            bool isServiceReady,
            bool isInventorySaveRegistered,
            bool isEquipmentSaveRegistered)
        {
            SelectedSlotId = selectedSlotId ?? string.Empty;
            ListedSaveSlotsSummary = listedSaveSlotsSummary ?? string.Empty;
            LastOperationSummary = lastOperationSummary ?? string.Empty;
            ShortenedSavePath = shortenedSavePath ?? string.Empty;
            IsServiceReady = isServiceReady;
            IsInventorySaveRegistered = isInventorySaveRegistered;
            IsEquipmentSaveRegistered = isEquipmentSaveRegistered;
        }

        #endregion

        #region Properties

        public string SelectedSlotId { get; }

        public string ListedSaveSlotsSummary { get; }

        public string LastOperationSummary { get; }

        public string ShortenedSavePath { get; }

        public bool IsServiceReady { get; }

        public bool IsInventorySaveRegistered { get; }

        public bool IsEquipmentSaveRegistered { get; }

        #endregion
    }
}
