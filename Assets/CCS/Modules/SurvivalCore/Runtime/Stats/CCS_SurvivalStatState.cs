// =============================================================================
// SCRIPT: CCS_SurvivalStatState
// CATEGORY: Survival / Runtime / SurvivalCore / Stats
// PURPOSE: Mutable runtime stat state with clamped current, min, and max values.
// PLACEMENT: Owned by CCS_SurvivalCoreService. Not a MonoBehaviour.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Normalized value is 0-1 across min/max span.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public sealed class CCS_SurvivalStatState
    {
        #region Variables

        private float currentValue;
        private float minValue;
        private float maxValue;

        #endregion

        #region Public Methods

        public CCS_SurvivalStatState(CCS_SurvivalStatType statType, float minValue, float maxValue, float startingValue)
        {
            StatType = statType;
            minValue = minValue;
            maxValue = maxValue;
            currentValue = CCS_SurvivalStatUtility.ClampValue(startingValue, minValue, maxValue);
        }

        public CCS_SurvivalStatSnapshot ToSnapshot()
        {
            return new CCS_SurvivalStatSnapshot(StatType, currentValue, minValue, maxValue);
        }

        public void SetCurrent(float value)
        {
            currentValue = CCS_SurvivalStatUtility.ClampValue(value, minValue, maxValue);
        }

        public void ApplyModifier(CCS_SurvivalStatModifier modifier)
        {
            currentValue = CCS_SurvivalStatUtility.ApplyModifier(currentValue, minValue, maxValue, modifier);
        }

        public void ApplyDelta(float delta)
        {
            currentValue = CCS_SurvivalStatUtility.ClampValue(currentValue + delta, minValue, maxValue);
        }

        public bool IsAtOrBelowMin()
        {
            return currentValue <= minValue + CCS_SurvivalStatUtility.DepletionEpsilon;
        }

        public bool IsAtOrAboveMax()
        {
            return currentValue >= maxValue - CCS_SurvivalStatUtility.DepletionEpsilon;
        }

        #endregion

        #region Properties

        public CCS_SurvivalStatType StatType { get; }

        public float CurrentValue => currentValue;

        public float MinValue => minValue;

        public float MaxValue => maxValue;

        public float NormalizedValue => CCS_SurvivalStatUtility.GetNormalizedValue(currentValue, minValue, maxValue);

        #endregion
    }
}
