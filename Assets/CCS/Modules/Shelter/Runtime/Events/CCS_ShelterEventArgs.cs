// =============================================================================
// SCRIPT: CCS_ShelterEventArgs
// CATEGORY: Modules / Shelter / Runtime / Events
// PURPOSE: Event payload for shelter lifecycle notifications.
// PLACEMENT: Raised by CCS_ShelterService on enter, exit, and state changes.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Snapshot is read-only. Safe when service is unavailable.
// =============================================================================

namespace CCS.Modules.Shelter
{
    public sealed class CCS_ShelterEventArgs
    {
        #region Public Methods

        public CCS_ShelterEventArgs(CCS_ShelterSnapshot snapshot, string message)
        {
            Snapshot = snapshot;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_ShelterSnapshot Snapshot { get; }

        public string Message { get; }

        #endregion
    }
}
