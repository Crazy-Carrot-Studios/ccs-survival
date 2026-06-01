using CCS.Modules.Wildlife;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_CombatValidationUtility
// CATEGORY: Modules / Combat / Runtime / Validation
// PURPOSE: Runtime-safe validation for combat profiles and weapon tuning rules.
// PLACEMENT: Used by editor validators and CCS_CombatService preflight checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Runtime-only; no editor APIs.
// =============================================================================

namespace CCS.Modules.Combat
{
    public static class CCS_CombatValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_CombatProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Combat profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.AttackCooldownSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Attack cooldown must be non-negative.");
            }

            if (profile.HitSphereRadius <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Hit sphere radius must be greater than zero.");
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

            if (profile.RabbitCarcassDefinition == null || profile.DeerCarcassDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Combat profile must reference rabbit and deer carcass definitions.");
            }

            if (profile.WildlifeProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Combat profile must reference a wildlife harvest profile.");
            }

            return CCS_SurvivalValidationResult.Pass("Combat profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateSpeciesSettings(
            string speciesName,
            CCS_CombatWildlifeSpeciesSettings settings)
        {
            if (settings.maxHealth <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail($"{speciesName} max health must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(settings.carcassObjectName))
            {
                return CCS_SurvivalValidationResult.Fail($"{speciesName} carcass object name is required.");
            }

            return CCS_SurvivalValidationResult.Pass($"{speciesName} combat settings validated.");
        }

        #endregion
    }
}
