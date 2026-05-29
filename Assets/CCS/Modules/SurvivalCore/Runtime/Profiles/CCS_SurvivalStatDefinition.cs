using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalStatDefinition
// CATEGORY: Survival / Runtime / SurvivalCore / Profiles
// PURPOSE: ScriptableObject-serialized min, max, and starting values for one survival stat.
// PLACEMENT: Listed on CCS_SurvivalCoreProfile.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Configuration only. Validated by CCS_SurvivalCoreValidationUtility.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    [Serializable]
    public sealed class CCS_SurvivalStatDefinition
    {
        #region Variables

        [Tooltip("Survival stat channel this definition configures.")]
        [SerializeField] private CCS_SurvivalStatType statType = CCS_SurvivalStatType.Health;

        [Tooltip("Minimum allowed value.")]
        [SerializeField] private float minValue;

        [Tooltip("Maximum allowed value.")]
        [SerializeField] private float maxValue = 100f;

        [Tooltip("Starting value when survival core initializes.")]
        [SerializeField] private float startingValue = 100f;

        #endregion

        #region Properties

        public CCS_SurvivalStatType StatType => statType;

        public float MinValue => minValue;

        public float MaxValue => maxValue;

        public float StartingValue => startingValue;

        #endregion
    }
}
