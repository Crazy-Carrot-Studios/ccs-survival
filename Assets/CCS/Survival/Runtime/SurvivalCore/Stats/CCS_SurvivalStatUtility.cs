using System;

// =============================================================================
// SCRIPT: CCS_SurvivalStatUtility
// CATEGORY: Survival / Runtime / SurvivalCore / Stats
// PURPOSE: Shared clamp, normalization, and modifier helpers for survival stats.
// PLACEMENT: Static utility. Not attached to GameObjects.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Safe when min equals max (returns 0 normalized).
// =============================================================================

namespace CCS.Survival.SurvivalCore
{
    public static class CCS_SurvivalStatUtility
    {
        public const float DepletionEpsilon = 0.0001f;

        #region Public Methods

        public static float ClampValue(float value, float minValue, float maxValue)
        {
            if (maxValue < minValue)
            {
                float swap = minValue;
                minValue = maxValue;
                maxValue = swap;
            }

            return Math.Clamp(value, minValue, maxValue);
        }

        public static float GetNormalizedValue(float currentValue, float minValue, float maxValue)
        {
            if (maxValue <= minValue)
            {
                return 0f;
            }

            return ClampValue((currentValue - minValue) / (maxValue - minValue), 0f, 1f);
        }

        public static float ApplyModifier(
            float currentValue,
            float minValue,
            float maxValue,
            CCS_SurvivalStatModifier modifier)
        {
            float result = modifier.IsMultiplicative
                ? currentValue * modifier.Amount
                : currentValue + modifier.Amount;

            return ClampValue(result, minValue, maxValue);
        }

        public static bool IsValidRange(float minValue, float maxValue)
        {
            return maxValue >= minValue;
        }

        public static bool IsValidStartingValue(float startingValue, float minValue, float maxValue)
        {
            if (!IsValidRange(minValue, maxValue))
            {
                return false;
            }

            return startingValue >= minValue && startingValue <= maxValue;
        }

        public static bool IsValidDecayRate(float changePerSecond)
        {
            return !float.IsNaN(changePerSecond) && !float.IsInfinity(changePerSecond) && changePerSecond >= 0f;
        }

        #endregion
    }
}
