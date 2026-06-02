using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SettlementValidationUtility
// CATEGORY: Modules / Settlements / Runtime / Validation
// PURPOSE: Profile and definition validation for settlement module startup.
// PLACEMENT: Used by editor validators and runtime service initialization.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.8.0 frontier settlement foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SettlementProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_SettlementDefinition[] definitions = profile.SettlementDefinitions;
            if (definitions == null || definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement profile requires at least one settlement definition.");
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementDefinition definition = definitions[index];
                if (definition == null)
                {
                    return CCS_SurvivalValidationResult.Fail($"Settlement definition at index {index} is null.");
                }

                if (string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    return CCS_SurvivalValidationResult.Fail("Settlement definition settlementId is required.");
                }

                if (string.IsNullOrWhiteSpace(definition.DisplayName))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement definition '{definition.SettlementId}' displayName is required.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Settlement profile validated.");
        }

        public static bool IsVendorBackedServicePoint(CCS_SettlementServicePointType servicePointType)
        {
            switch (servicePointType)
            {
                case CCS_SettlementServicePointType.GeneralStore:
                case CCS_SettlementServicePointType.Stable:
                case CCS_SettlementServicePointType.Gunsmith:
                    return true;
                default:
                    return false;
            }
        }
    }
}
