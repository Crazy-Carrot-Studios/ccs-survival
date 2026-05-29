using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapper
// CATEGORY: Survival / Runtime / Development / Bootstrap
// PURPOSE: Minimal optional scene bootstrap helper for future required prefab/service validation wiring.
// PLACEMENT: Optional child under survival composition root. Safe when profile is unassigned.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Does not instantiate gameplay modules. Validates optional profile expectations only.
// =============================================================================

namespace CCS.Survival.Development
{
    public sealed class CCS_SurvivalSceneBootstrapper : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_SurvivalSceneBootstrapper]";

        #region Variables

        [Header("Bootstrap Profile")]
        [Tooltip("Optional scene bootstrap profile for development validation expectations.")]
        [SerializeField] private CCS_SurvivalSceneBootstrapProfile bootstrapProfile;

        [Header("Diagnostics")]
        [Tooltip("Emit development bootstrap logs when validating profile expectations.")]
        [SerializeField] private bool enableDebugLogs;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ValidateOptionalProfile();
        }

        #endregion

        #region Public Methods

        public CCS_SurvivalSceneBootstrapProfile BootstrapProfile => bootstrapProfile;

        public CCS_SurvivalValidationResult ValidateOptionalProfile()
        {
            if (bootstrapProfile == null)
            {
                CCS_Logger.Log(LogPrefix, "No scene bootstrap profile assigned. Skipping development profile validation.", enableDebugLogs);
                return CCS_SurvivalValidationResult.Pass("No development scene bootstrap profile assigned.");
            }

            CCS_SurvivalValidationResult profileValidation =
                CCS_SurvivalProfileValidationUtility.ValidateProfile(bootstrapProfile);
            if (!profileValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogPrefix, profileValidation.Message);
                return profileValidation;
            }

            CCS_SurvivalValidationResult requirementValidation =
                CCS_SurvivalSceneBootstrapValidationUtility.ValidateProfileRequirements(bootstrapProfile);
            LogValidation(requirementValidation);
            return requirementValidation;
        }

        #endregion

        #region Private Methods

        private void LogValidation(CCS_SurvivalValidationResult validationResult)
        {
            if (!validationResult.IsSuccess)
            {
                CCS_Logger.LogWarning(LogPrefix, validationResult.Message);
                return;
            }

            if (validationResult.IsWarning)
            {
                CCS_Logger.LogWarning(LogPrefix, validationResult.Message);
                return;
            }

            CCS_Logger.Log(LogPrefix, validationResult.Message, enableDebugLogs);
        }

        #endregion
    }
}
