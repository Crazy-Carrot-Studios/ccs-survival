// =============================================================================
// SCRIPT: CCS_TimeOfDayEvents
// CATEGORY: Modules / TimeOfDay / Runtime / Events
// PURPOSE: Event delegate definitions for global game clock lifecycle.
// PLACEMENT: Subscribed by HUD/debug presenters and future gameplay systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Event name constants for diagnostics and future tooling.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public static class CCS_TimeOfDayEvents
    {
        #region Variables

        public const string TimeChanged = "TimeChanged";

        public const string HourChanged = "HourChanged";

        public const string DayChanged = "DayChanged";

        public const string PhaseChanged = "PhaseChanged";

        public const string TimePaused = "TimePaused";

        public const string TimeResumed = "TimeResumed";

        #endregion
    }

    public delegate void TimeOfDayChangedHandler(CCS_TimeOfDayEventArgs eventArgs);

    public delegate void TimeOfDayHourChangedHandler(CCS_TimeOfDayEventArgs eventArgs);

    public delegate void TimeOfDayDayChangedHandler(CCS_TimeOfDayEventArgs eventArgs);

    public delegate void TimeOfDayPhaseChangedHandler(CCS_TimeOfDayEventArgs eventArgs);

    public delegate void TimeOfDayPausedHandler(CCS_TimeOfDayEventArgs eventArgs);

    public delegate void TimeOfDayResumedHandler(CCS_TimeOfDayEventArgs eventArgs);
}
