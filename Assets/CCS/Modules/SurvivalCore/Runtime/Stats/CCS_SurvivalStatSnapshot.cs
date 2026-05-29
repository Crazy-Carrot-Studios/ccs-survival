// =============================================================================
// SCRIPT: CCS_SurvivalStatSnapshot
// CATEGORY: Survival / Runtime / SurvivalCore / Stats
// PURPOSE: Read-only stat value snapshot for queries and event payloads.
// PLACEMENT: Returned by CCS_SurvivalCoreService. Not mutated after creation.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: No UI dependencies.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public readonly struct CCS_SurvivalStatSnapshot
    {
        #region Public Methods

        public CCS_SurvivalStatSnapshot(CCS_SurvivalStatType statType, float currentValue, float minValue, float maxValue)
        {
            StatType = statType;
            CurrentValue = currentValue;
            MinValue = minValue;
            MaxValue = maxValue;
            NormalizedValue = CCS_SurvivalStatUtility.GetNormalizedValue(currentValue, minValue, maxValue);
        }

        #endregion

        #region Properties

        public CCS_SurvivalStatType StatType { get; }

        public float CurrentValue { get; }

        public float MinValue { get; }

        public float MaxValue { get; }

        public float NormalizedValue { get; }

        #endregion
    }
}
