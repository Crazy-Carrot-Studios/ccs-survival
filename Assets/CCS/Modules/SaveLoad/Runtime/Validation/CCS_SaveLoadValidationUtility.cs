using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SaveLoadValidationUtility
// CATEGORY: Modules / SaveLoad / Runtime / Validation
// PURPOSE: Runtime-safe validation for save/load profiles and slot identifiers.
// PLACEMENT: Used by editor validators and future bootstrap checks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public static class CCS_SaveLoadValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SaveLoadProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Save/load profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.MaxSaveSlots <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Max save slots must be greater than zero.");
            }

            if (profile.AutoSaveIntervalSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Auto save interval cannot be negative.");
            }

            return CCS_SurvivalValidationResult.Pass("Save/load profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateSlotId(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId))
            {
                return CCS_SurvivalValidationResult.Fail("Save slot id is empty.");
            }

            string sanitized = CCS_SavePathUtility.SanitizeSlotId(slotId);
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                return CCS_SurvivalValidationResult.Fail("Save slot id is invalid after sanitization.");
            }

            return CCS_SurvivalValidationResult.Pass("Save slot id validated.");
        }

        #endregion
    }
}
