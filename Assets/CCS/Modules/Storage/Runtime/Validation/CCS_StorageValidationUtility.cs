using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_StorageValidationUtility
// CATEGORY: Modules / Storage / Runtime / Validation
// PURPOSE: Profile validation helpers for storage module startup configuration.
// PLACEMENT: Used by editor validators and runtime service initialization.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.2 storage container foundation.
// =============================================================================

namespace CCS.Modules.Storage
{
    public static class CCS_StorageValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_StorageProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Storage profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.DefaultContainerDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Default container definition is required.");
            }

            if (string.IsNullOrWhiteSpace(profile.DefaultContainerDefinition.ContainerId))
            {
                return CCS_SurvivalValidationResult.Fail("Default container definition containerId is required.");
            }

            if (profile.DefaultContainerDefinition.PrefabReference == null)
            {
                return CCS_SurvivalValidationResult.Fail("Default container definition prefabReference is required.");
            }

            return CCS_SurvivalValidationResult.Pass("Storage profile validated.");
        }

        #endregion
    }
}
