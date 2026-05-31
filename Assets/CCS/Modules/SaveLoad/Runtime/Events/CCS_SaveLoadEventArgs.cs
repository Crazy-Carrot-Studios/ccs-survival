// =============================================================================
// SCRIPT: CCS_SaveLoadEventArgs
// CATEGORY: Modules / SaveLoad / Runtime / Events
// PURPOSE: Event payload for save/load lifecycle notifications.
// PLACEMENT: Passed to CCS_SaveLoadService event subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI or gameplay module references in 0.6.0 foundation.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public sealed class CCS_SaveLoadEventArgs
    {
        #region Public Methods

        public CCS_SaveLoadEventArgs(
            string slotId,
            string saveId,
            string message = "")
        {
            SlotId = slotId ?? string.Empty;
            SaveId = saveId ?? string.Empty;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public string SlotId { get; }

        public string SaveId { get; }

        public string Message { get; }

        #endregion
    }
}
