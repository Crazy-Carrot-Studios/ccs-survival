using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_RegionValidationUtility
// CATEGORY: Modules / Regions / Runtime / Validation
// PURPOSE: Profile and definition validation for region module startup.
// PLACEMENT: Used by editor validators and runtime service initialization.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.9.0 frontier region foundation.
// =============================================================================

namespace CCS.Modules.Regions
{
    public static class CCS_RegionValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_RegionProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Region profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_RegionDefinition[] definitions = profile.RegionDefinitions;
            if (definitions == null || definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Region profile requires at least one region definition.");
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_RegionDefinition definition = definitions[index];
                if (definition == null)
                {
                    return CCS_SurvivalValidationResult.Fail($"Region definition at index {index} is null.");
                }

                if (string.IsNullOrWhiteSpace(definition.RegionId))
                {
                    return CCS_SurvivalValidationResult.Fail("Region definition regionId is required.");
                }

                if (string.IsNullOrWhiteSpace(definition.DisplayName))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Region definition '{definition.RegionId}' displayName is required.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Region profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateSettlementAssignments(
            CCS_RegionProfile regionProfile,
            string[] knownSettlementIds)
        {
            if (regionProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Region profile is null.");
            }

            CCS_RegionDefinition[] definitions = regionProfile.RegionDefinitions;
            for (int definitionIndex = 0; definitionIndex < definitions.Length; definitionIndex++)
            {
                CCS_RegionDefinition definition = definitions[definitionIndex];
                if (definition == null)
                {
                    continue;
                }

                string[] settlementIds = definition.SettlementIds;
                for (int settlementIndex = 0; settlementIndex < settlementIds.Length; settlementIndex++)
                {
                    string settlementId = settlementIds[settlementIndex];
                    if (string.IsNullOrWhiteSpace(settlementId))
                    {
                        continue;
                    }

                    if (knownSettlementIds == null || knownSettlementIds.Length == 0)
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Region '{definition.RegionId}' references settlement '{settlementId}' but no settlement catalog was supplied.");
                    }

                    bool found = false;
                    for (int knownIndex = 0; knownIndex < knownSettlementIds.Length; knownIndex++)
                    {
                        if (string.Equals(settlementId, knownSettlementIds[knownIndex], System.StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Region '{definition.RegionId}' references unknown settlement id '{settlementId}'.");
                    }
                }
            }

            return CCS_SurvivalValidationResult.Pass("Region settlement assignments validated.");
        }
    }
}
