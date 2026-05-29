using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapValidationUtility
// CATEGORY: Survival / Runtime / Development / Bootstrap
// PURPOSE: Development-layer scene bootstrap validation helpers for profiles and active scene checks.
// PLACEMENT: Invoked by editor menus and CCS_SurvivalSceneBootstrapper. No gameplay rules.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Complements foundation CCS.Survival scene bootstrap validation without replacing it.
// =============================================================================

namespace CCS.Survival.Development
{
    public static class CCS_SurvivalSceneBootstrapValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfileRequirements(CCS_SurvivalSceneBootstrapProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Pass("No development scene bootstrap profile to validate.");
            }

            CCS_SurvivalValidationResult profileValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!profileValidation.IsSuccess)
            {
                return profileValidation;
            }

            if (profile.RequireRuntimeHost)
            {
                CCS_RuntimeHost[] runtimeHosts = Object.FindObjectsByType<CCS_RuntimeHost>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

                if (runtimeHosts.Length != CCS_SurvivalRuntimeConstants.ExpectedRuntimeHostCount)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Expected {CCS_SurvivalRuntimeConstants.ExpectedRuntimeHostCount} runtime host, found {runtimeHosts.Length}.");
                }
            }

            if (profile.RequireSurvivalBootstrap)
            {
                CCS_SurvivalBootstrap[] survivalBootstraps = Object.FindObjectsByType<CCS_SurvivalBootstrap>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

                if (survivalBootstraps.Length != CCS_SurvivalRuntimeConstants.ExpectedSurvivalBootstrapCount)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Expected {CCS_SurvivalRuntimeConstants.ExpectedSurvivalBootstrapCount} survival bootstrap, found {survivalBootstraps.Length}.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Development scene bootstrap profile requirements validated.");
        }

        public static CCS_SurvivalValidationResult ValidateActiveSceneBootstrap(CCS_SurvivalSceneBootstrapProfile profile)
        {
            CCS_SurvivalValidationResult profileRequirements = ValidateProfileRequirements(profile);
            if (!profileRequirements.IsSuccess)
            {
                return profileRequirements;
            }

            CCS_SurvivalBootstrap survivalBootstrap = Object.FindFirstObjectByType<CCS_SurvivalBootstrap>();
            if (survivalBootstrap == null)
            {
                return CCS_SurvivalValidationResult.Fail("Active scene is missing CCS_SurvivalBootstrap.");
            }

            CCS_SurvivalRuntimeContext survivalContext = survivalBootstrap.SurvivalContext;
            if (survivalContext == null)
            {
                return CCS_SurvivalValidationResult.Warn("Survival bootstrap context is not available yet.");
            }

            CCS_SurvivalValidationResult foundationValidation =
                CCS.Survival.CCS_SurvivalSceneBootstrapValidationUtility.ValidateSceneBootstrap(
                    survivalContext,
                    survivalBootstrap);

            if (!foundationValidation.IsSuccess)
            {
                return foundationValidation;
            }

            if (foundationValidation.IsWarning)
            {
                return foundationValidation;
            }

            return CCS_SurvivalValidationResult.Pass("Active scene development bootstrap validation passed.");
        }

        #endregion
    }
}
