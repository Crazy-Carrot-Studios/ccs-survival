// =============================================================================
// SCRIPT: CCS_WeatherTransitionMode
// CATEGORY: Modules / Weather / Runtime / Definitions
// PURPOSE: Defines how weather changes are applied at runtime.
// PLACEMENT: Used by CCS_WeatherService.SetWeather().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Timed transitions prepare future blending hooks without VFX.
// =============================================================================

namespace CCS.Modules.Weather
{
    public enum CCS_WeatherTransitionMode
    {
        Instant = 0,
        Timed = 1
    }
}
