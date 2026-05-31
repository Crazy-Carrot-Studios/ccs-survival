// =============================================================================
// SCRIPT: CCS_WeatherSaveData
// CATEGORY: Modules / Weather / Runtime / Data
// PURPOSE: Versioned save payload for global weather state persistence.
// PLACEMENT: Serialized by CCS_WeatherService.CaptureState().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Restored after time-of-day in ModuleRestoreOrder.
// =============================================================================

namespace CCS.Modules.Weather
{
    [System.Serializable]
    public sealed class CCS_WeatherSaveData
    {
        #region Variables

        public const int CurrentSaveDataVersion = 1;

        public int saveDataVersion = CurrentSaveDataVersion;

        public CCS_WeatherType currentWeather = CCS_WeatherType.Clear;

        public CCS_WeatherType previousWeather = CCS_WeatherType.Clear;

        public CCS_WeatherType targetWeather = CCS_WeatherType.Clear;

        public float transitionProgress;

        public float remainingDuration;

        public bool isPaused;

        #endregion
    }
}
