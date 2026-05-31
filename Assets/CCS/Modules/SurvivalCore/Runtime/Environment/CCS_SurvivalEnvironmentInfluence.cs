// =============================================================================
// SCRIPT: CCS_SurvivalEnvironmentInfluence
// CATEGORY: Modules / SurvivalCore / Runtime / Environment
// PURPOSE: Read-only environment influence snapshot for stat pressure and HUD debug.
// PLACEMENT: Produced by CCS_SurvivalCoreService from Environment Effects snapshots.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Per-second delta fields are rates applied during TickSurvival. No Health influence.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public readonly struct CCS_SurvivalEnvironmentInfluence
    {
        #region Public Methods

        public CCS_SurvivalEnvironmentInfluence(
            float ambientTemperature,
            float wetness,
            float exposure,
            float calculatedTemperatureDelta,
            float calculatedFatigueDelta,
            float calculatedThirstDelta)
        {
            AmbientTemperature = ambientTemperature;
            Wetness = wetness < 0f ? 0f : wetness;
            Exposure = exposure < 0f ? 0f : exposure;
            CalculatedTemperatureDelta = calculatedTemperatureDelta;
            CalculatedFatigueDelta = calculatedFatigueDelta;
            CalculatedThirstDelta = calculatedThirstDelta;
        }

        public static CCS_SurvivalEnvironmentInfluence Empty =>
            new CCS_SurvivalEnvironmentInfluence(0f, 0f, 0f, 0f, 0f, 0f);

        #endregion

        #region Properties

        public float AmbientTemperature { get; }

        public float Wetness { get; }

        public float Exposure { get; }

        public float CalculatedTemperatureDelta { get; }

        public float CalculatedFatigueDelta { get; }

        public float CalculatedThirstDelta { get; }

        #endregion
    }
}
