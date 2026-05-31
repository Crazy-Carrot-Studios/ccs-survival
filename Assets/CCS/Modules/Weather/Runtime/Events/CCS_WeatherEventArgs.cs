// =============================================================================
// SCRIPT: CCS_WeatherEventArgs
// CATEGORY: Modules / Weather / Runtime / Events
// PURPOSE: Event payload carrying weather snapshots and diagnostic messages.
// PLACEMENT: Passed to weather service event subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Read-only snapshot reference for HUD and future systems.
// =============================================================================

namespace CCS.Modules.Weather
{
    public sealed class CCS_WeatherEventArgs
    {
        #region Public Methods

        public CCS_WeatherEventArgs(CCS_WeatherSnapshot snapshot, string message)
        {
            Snapshot = snapshot;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_WeatherSnapshot Snapshot { get; }

        public string Message { get; }

        #endregion
    }
}
