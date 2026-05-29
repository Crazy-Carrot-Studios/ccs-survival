using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalSettingsService
// CATEGORY: Survival / Runtime / Development / Settings
// PURPOSE: Placeholder settings service for future preference modules without requiring a profile asset.
// PLACEMENT: Registered manually by future bootstrap wiring. Instance-owned; not a singleton.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Safe defaults when profile is null. No settings UI in 0.3.6.
// =============================================================================

namespace CCS.Survival.Development
{
    public sealed class CCS_SurvivalSettingsService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SurvivalSettingsService]";
        private const float DefaultMasterVolume = 1f;
        private const float DefaultInputSensitivity = 1f;

        #region Variables

        private CCS_SurvivalSettingsProfile activeProfile;
        private bool isInitialized;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        #endregion

        #region Public Methods

        public CCS_SurvivalSettingsService(CCS_SurvivalSettingsProfile profile)
        {
            activeProfile = profile;
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            LogProfileState();
        }

        public void SetProfile(CCS_SurvivalSettingsProfile profile)
        {
            activeProfile = profile;

            if (isInitialized)
            {
                LogProfileState();
            }
        }

        public bool HasProfile()
        {
            return activeProfile != null;
        }

        public int GetDefaultQualityTier()
        {
            return activeProfile != null ? activeProfile.DefaultQualityTier : 0;
        }

        public float GetMasterVolume()
        {
            return activeProfile != null ? activeProfile.MasterVolume : DefaultMasterVolume;
        }

        public float GetInputSensitivity()
        {
            return activeProfile != null ? activeProfile.InputSensitivity : DefaultInputSensitivity;
        }

        public bool GetSubtitlesEnabled()
        {
            return activeProfile != null && activeProfile.EnableSubtitles;
        }

        #endregion

        #region Private Methods

        private void LogProfileState()
        {
            if (activeProfile == null)
            {
                CCS_Logger.Log(LogPrefix, "No settings profile assigned. Using safe defaults.");
                return;
            }

            CCS_Logger.Log(
                LogPrefix,
                $"Settings profile active: {activeProfile.ProfileDisplayName} ({activeProfile.ProfileId}).");
        }

        #endregion
    }
}
