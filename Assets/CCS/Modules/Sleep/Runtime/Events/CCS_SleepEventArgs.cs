// =============================================================================
// SCRIPT: CCS_SleepEventArgs
// CATEGORY: Modules / Sleep / Runtime / Events
// PURPOSE: Event payload for sleep completed and failed notifications.
// PLACEMENT: Raised by CCS_SleepService event handlers.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No UI references in 0.9.6 foundation.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_SleepEventArgs
    {
        #region Constructors

        public CCS_SleepEventArgs(CCS_SleepResult result, CCS_SleepSnapshot snapshot, string message)
        {
            Result = result;
            Snapshot = snapshot ?? CCS_SleepSnapshot.Empty;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_SleepResult Result { get; }

        public CCS_SleepSnapshot Snapshot { get; }

        public string Message { get; }

        #endregion
    }
}
