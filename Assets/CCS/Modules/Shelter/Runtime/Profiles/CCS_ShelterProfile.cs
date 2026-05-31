using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ShelterProfile
// CATEGORY: Modules / Shelter / Runtime / Profiles
// PURPOSE: Tuning profile for default shelter protection and volume policy.
// PLACEMENT: Assets/CCS/Survival/Profiles/Shelter/ (project shell configuration).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation only. No building placement or structure durability.
// =============================================================================

namespace CCS.Modules.Shelter
{
    [CreateAssetMenu(
        fileName = "CCS_ShelterProfile",
        menuName = "CCS/Survival/Shelter/Shelter Profile")]
    public sealed class CCS_ShelterProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Default Protection")]
        [Tooltip("Default wetness protection applied while sheltered.")]
        [SerializeField] private float defaultWetnessProtection = 1f;

        [Tooltip("Default exposure protection applied while sheltered.")]
        [SerializeField] private float defaultExposureProtection = 0.6f;

        [Tooltip("Default temperature protection placeholder applied while sheltered.")]
        [SerializeField] private float defaultTemperatureProtection = 1f;

        [Tooltip("Multiplier applied to shelter protection values.")]
        [SerializeField] private float defaultProtectionMultiplier = 1f;

        [Header("Volume Rules")]
        [Tooltip("When enabled, shelter volumes require trigger colliders for entry.")]
        [SerializeField] private bool requireTriggerVolume = true;

        #endregion

        #region Properties

        public float DefaultWetnessProtection => defaultWetnessProtection < 0f ? 0f : defaultWetnessProtection;

        public float DefaultExposureProtection => defaultExposureProtection < 0f ? 0f : defaultExposureProtection;

        public float DefaultTemperatureProtection => defaultTemperatureProtection;

        public float DefaultProtectionMultiplier =>
            defaultProtectionMultiplier <= 0f ? 1f : defaultProtectionMultiplier;

        public bool RequireTriggerVolume => requireTriggerVolume;

        #endregion
    }
}
