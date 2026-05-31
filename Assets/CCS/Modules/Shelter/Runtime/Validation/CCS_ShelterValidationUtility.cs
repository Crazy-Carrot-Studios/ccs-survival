using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ShelterValidationUtility
// CATEGORY: Modules / Shelter / Runtime / Validation
// PURPOSE: Profile and protection validation helpers for runtime and editor checks.
// PLACEMENT: Used by shelter service initialization and editor validation pipeline.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Shelter
{
    public static class CCS_ShelterValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_ShelterProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Shelter profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            return ValidateProtectionValues(
                profile.DefaultWetnessProtection,
                profile.DefaultExposureProtection,
                profile.DefaultProtectionMultiplier);
        }

        public static CCS_SurvivalValidationResult ValidateProtectionValues(
            float wetnessProtection,
            float exposureProtection,
            float protectionMultiplier)
        {
            if (wetnessProtection < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Wetness protection cannot be negative.");
            }

            if (exposureProtection < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Exposure protection cannot be negative.");
            }

            if (protectionMultiplier <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Protection multiplier must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass("Shelter protection values validated.");
        }

        public static CCS_SurvivalValidationResult ValidateShelterVolume(CCS_ShelterVolume shelterVolume)
        {
            if (shelterVolume == null)
            {
                return CCS_SurvivalValidationResult.Fail("Shelter volume is null.");
            }

            if (string.IsNullOrWhiteSpace(shelterVolume.ShelterId))
            {
                return CCS_SurvivalValidationResult.Fail("Shelter volume requires a shelter ID.");
            }

            return ValidateProtectionValues(
                shelterVolume.WetnessProtection,
                shelterVolume.ExposureProtection,
                shelterVolume.ProtectionMultiplier);
        }

        #endregion
    }
}
