using CCS.Modules.EnvironmentEffects;

// =============================================================================
// SCRIPT: CCS_SurvivalEnvironmentInfluenceUtility
// CATEGORY: Modules / SurvivalCore / Runtime / Environment
// PURPOSE: Calculates conservative environment pressure rates for Survival Core stats.
// PLACEMENT: Used by CCS_SurvivalCoreService during TickSurvival and influence refresh.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Temperature, fatigue, and thirst only. No Health or damage systems.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public static class CCS_SurvivalEnvironmentInfluenceUtility
    {
        #region Public Methods

        public static CCS_SurvivalEnvironmentInfluence Calculate(
            CCS_EnvironmentSnapshot environmentSnapshot,
            CCS_SurvivalCoreProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalEnvironmentInfluence.Empty;
            }

            float ambientTemperature = environmentSnapshot.EffectiveTemperature;
            float wetness = environmentSnapshot.EffectiveWetness;
            float exposure = environmentSnapshot.EffectiveExposure;

            float temperatureDeltaPerSecond = 0f;
            if (ambientTemperature > 0f)
            {
                temperatureDeltaPerSecond = ambientTemperature * profile.TemperatureRecoveryRate;
            }
            else if (ambientTemperature < 0f)
            {
                temperatureDeltaPerSecond = ambientTemperature * profile.TemperatureDecayRate;
            }

            float fatigueDeltaPerSecond = exposure * profile.ExposureFatigueMultiplier;
            float thirstDeltaPerSecond = -wetness * profile.WetnessThirstMultiplier;

            return new CCS_SurvivalEnvironmentInfluence(
                ambientTemperature,
                wetness,
                exposure,
                temperatureDeltaPerSecond,
                fatigueDeltaPerSecond,
                thirstDeltaPerSecond);
        }

        public static string FormatInfluenceDisplay(CCS_SurvivalEnvironmentInfluence influence)
        {
            return
                $"Temp Δ: {influence.CalculatedTemperatureDelta:0.###}\n" +
                $"Fatigue Δ: {influence.CalculatedFatigueDelta:0.###}\n" +
                $"Thirst Δ: {influence.CalculatedThirstDelta:0.###}";
        }

        public static bool HasMeaningfulChange(
            CCS_SurvivalEnvironmentInfluence previous,
            CCS_SurvivalEnvironmentInfluence current)
        {
            return !ApproximatelyEqual(previous.AmbientTemperature, current.AmbientTemperature)
                || !ApproximatelyEqual(previous.Wetness, current.Wetness)
                || !ApproximatelyEqual(previous.Exposure, current.Exposure)
                || !ApproximatelyEqual(previous.CalculatedTemperatureDelta, current.CalculatedTemperatureDelta)
                || !ApproximatelyEqual(previous.CalculatedFatigueDelta, current.CalculatedFatigueDelta)
                || !ApproximatelyEqual(previous.CalculatedThirstDelta, current.CalculatedThirstDelta);
        }

        #endregion

        #region Private Methods

        private static bool ApproximatelyEqual(float left, float right)
        {
            return System.Math.Abs(left - right) <= CCS_SurvivalStatUtility.DepletionEpsilon;
        }

        #endregion
    }
}
