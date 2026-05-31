// =============================================================================
// SCRIPT: CCS_WeatherEvents
// CATEGORY: Modules / Weather / Runtime / Events
// PURPOSE: Event delegate definitions for global weather lifecycle.
// PLACEMENT: Subscribed by HUD/debug presenters and future environment systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Event name constants for diagnostics and future tooling.
// =============================================================================

namespace CCS.Modules.Weather
{
    public static class CCS_WeatherEvents
    {
        #region Variables

        public const string WeatherChanged = "WeatherChanged";

        public const string WeatherTransitionStarted = "WeatherTransitionStarted";

        public const string WeatherTransitionCompleted = "WeatherTransitionCompleted";

        public const string WeatherPaused = "WeatherPaused";

        public const string WeatherResumed = "WeatherResumed";

        #endregion
    }

    public delegate void WeatherChangedHandler(CCS_WeatherEventArgs eventArgs);

    public delegate void WeatherTransitionStartedHandler(CCS_WeatherEventArgs eventArgs);

    public delegate void WeatherTransitionCompletedHandler(CCS_WeatherEventArgs eventArgs);

    public delegate void WeatherPausedHandler(CCS_WeatherEventArgs eventArgs);

    public delegate void WeatherResumedHandler(CCS_WeatherEventArgs eventArgs);
}
