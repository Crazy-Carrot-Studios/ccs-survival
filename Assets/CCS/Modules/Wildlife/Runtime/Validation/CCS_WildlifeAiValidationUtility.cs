using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WildlifeAiValidationUtility
// CATEGORY: Modules / Wildlife / Runtime / Validation
// PURPOSE: Runtime-safe validation for passive wildlife AI profiles and species tuning.
// PLACEMENT: Used by editor validators and CCS_WildlifeAiService preflight checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Runtime-only; no editor APIs.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public static class CCS_WildlifeAiValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_WildlifeAiProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife AI profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_SurvivalValidationResult rabbitValidation =
                ValidateSpeciesSettings("Rabbit", profile.GetSpeciesSettings(CCS_WildlifeAiSpecies.Rabbit));
            if (!rabbitValidation.IsSuccess)
            {
                return rabbitValidation;
            }

            CCS_SurvivalValidationResult deerValidation =
                ValidateSpeciesSettings("Deer", profile.GetSpeciesSettings(CCS_WildlifeAiSpecies.Deer));
            if (!deerValidation.IsSuccess)
            {
                return deerValidation;
            }

            if (profile.MinimumIdleSeconds < 0f || profile.MaximumIdleSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Idle duration values must be non-negative.");
            }

            if (profile.MinimumIdleSeconds > profile.MaximumIdleSeconds)
            {
                return CCS_SurvivalValidationResult.Fail("Minimum idle seconds must be <= maximum idle seconds.");
            }

            if (profile.AlertDurationSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Alert duration must be non-negative.");
            }

            if (profile.FleeDestinationDistance <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Flee destination distance must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass("Wildlife AI profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateSpeciesSettings(
            string speciesName,
            CCS_WildlifeAiSpeciesSettings settings)
        {
            if (settings.wanderRadius <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail($"{speciesName} wander radius must be greater than zero.");
            }

            if (settings.fleeRadius <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail($"{speciesName} flee radius must be greater than zero.");
            }

            if (settings.moveSpeed <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail($"{speciesName} move speed must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass($"{speciesName} AI settings validated.");
        }

        #endregion
    }
}
