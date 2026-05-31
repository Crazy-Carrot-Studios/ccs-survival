// =============================================================================
// SCRIPT: CCS_TimeOfDayEventArgs
// CATEGORY: Modules / TimeOfDay / Runtime / Events
// PURPOSE: Event payload for time-of-day lifecycle notifications.
// PLACEMENT: Passed to OnTimeChanged, OnHourChanged, OnDayChanged, and related handlers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Carries read-only snapshot data. No gameplay mutation.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public sealed class CCS_TimeOfDayEventArgs
    {
        #region Public Methods

        public CCS_TimeOfDayEventArgs(CCS_GameTimeSnapshot snapshot, string message = "")
        {
            Snapshot = snapshot;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_GameTimeSnapshot Snapshot { get; }

        public string Message { get; }

        #endregion
    }
}
