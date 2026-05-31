// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsEvents
// CATEGORY: Modules / EnvironmentEffects / Runtime / Events
// PURPOSE: Event delegate definitions for environment simulation lifecycle.
// PLACEMENT: Subscribed by HUD/debug presenters and future Survival Core systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Event name constants for diagnostics and future tooling.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public static class CCS_EnvironmentEffectsEvents
    {
        #region Variables

        public const string EnvironmentChanged = "EnvironmentChanged";

        public const string TemperatureChanged = "TemperatureChanged";

        public const string WetnessChanged = "WetnessChanged";

        public const string ExposureChanged = "ExposureChanged";

        #endregion
    }

    public delegate void EnvironmentChangedHandler(CCS_EnvironmentEffectsEventArgs eventArgs);

    public delegate void EnvironmentTemperatureChangedHandler(CCS_EnvironmentEffectsEventArgs eventArgs);

    public delegate void EnvironmentWetnessChangedHandler(CCS_EnvironmentEffectsEventArgs eventArgs);

    public delegate void EnvironmentExposureChangedHandler(CCS_EnvironmentEffectsEventArgs eventArgs);
}
