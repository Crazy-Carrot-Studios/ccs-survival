// =============================================================================
// SCRIPT: CCS_EnvironmentState
// CATEGORY: Modules / EnvironmentEffects / Runtime / Data
// PURPOSE: Mutable environment simulation state owned by CCS_EnvironmentEffectsService.
// PLACEMENT: Internal to environment service calculation and save/restore flows.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Does not mutate Survival Core stats in 0.7.2 foundation.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public sealed class CCS_EnvironmentState
    {
        #region Variables

        public float AmbientTemperature;

        public float Wetness;

        public float Exposure;

        #endregion
    }
}
