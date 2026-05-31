// =============================================================================
// SCRIPT: CCS_WeatherType
// CATEGORY: Modules / Weather / Runtime / Definitions
// PURPOSE: Authoritative weather type identifiers for global weather state.
// PLACEMENT: Referenced by weather service, profiles, save data, and HUD.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No visual effects or audio in 0.7.1 foundation.
// =============================================================================

namespace CCS.Modules.Weather
{
    public enum CCS_WeatherType
    {
        Clear = 0,
        Cloudy = 1,
        Rain = 2,
        Storm = 3,
        Fog = 4
    }
}
