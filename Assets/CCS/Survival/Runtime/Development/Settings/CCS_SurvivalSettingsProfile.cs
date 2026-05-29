using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSettingsProfile
// CATEGORY: Survival / Runtime / Development / Settings
// PURPOSE: ScriptableObject placeholder for future graphics/audio/input/accessibility preferences.
// PLACEMENT: Assets/CCS/Survival/Settings/Development/ (future). Optional at runtime.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Configuration only. No settings UI in 0.3.6. Safe when unassigned.
// =============================================================================

namespace CCS.Survival.Development
{
    [CreateAssetMenu(
        fileName = "CCS_SurvivalSettingsProfile",
        menuName = "CCS/Survival/Development/Settings Profile")]
    public sealed class CCS_SurvivalSettingsProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Graphics Placeholder")]
        [Tooltip("Future master quality tier index.")]
        [SerializeField] private int defaultQualityTier;

        [Header("Audio Placeholder")]
        [Tooltip("Future master volume scalar (0-1).")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Header("Input Placeholder")]
        [Tooltip("Future input sensitivity scalar.")]
        [Range(0.1f, 3f)]
        [SerializeField] private float inputSensitivity = 1f;

        [Header("Accessibility Placeholder")]
        [Tooltip("Future subtitle enable default.")]
        [SerializeField] private bool enableSubtitles;

        #endregion

        #region Properties

        public int DefaultQualityTier => defaultQualityTier;

        public float MasterVolume => masterVolume;

        public float InputSensitivity => inputSensitivity;

        public bool EnableSubtitles => enableSubtitles;

        #endregion
    }
}
