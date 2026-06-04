using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ReputationValidationUtility
// CATEGORY: Modules / Reputation / Runtime / Validation
// PURPOSE: Shared profile/content validation for reputation bootstrap and runtime.
// PLACEMENT: Used by CCS_ReputationService and editor validators.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public static class CCS_ReputationValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_ReputationProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Reputation profile is null.");
            }

            CCS_ReputationDefinition[] definitions = profile.ReputationDefinitions;
            if (definitions == null || definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Reputation profile has no reputation definitions.");
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SurvivalValidationResult definitionResult = ValidateDefinition(definitions[index]);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }
            }

            if (!profile.TryGetDefinitionById(profile.DefaultSettlementReputationDefinitionId, out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Reputation profile default id '{profile.DefaultSettlementReputationDefinitionId}' was not found.");
            }

            if (string.IsNullOrWhiteSpace(profile.DefaultTradingPostSettlementId))
            {
                return CCS_SurvivalValidationResult.Fail("Reputation profile is missing defaultTradingPostSettlementId.");
            }

            return CCS_SurvivalValidationResult.Pass("Reputation profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateDefinition(CCS_ReputationDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Reputation definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.ReputationDefinitionId))
            {
                return CCS_SurvivalValidationResult.Fail("Reputation definition is missing reputationDefinitionId.");
            }

            if (definition.MinValue >= definition.MaxValue)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Reputation '{definition.ReputationDefinitionId}' minValue must be less than maxValue.");
            }

            if (definition.DefaultValue < definition.MinValue || definition.DefaultValue > definition.MaxValue)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Reputation '{definition.ReputationDefinitionId}' defaultValue must be within min/max range.");
            }

            if (definition.ScopeType == CCS_ReputationScopeType.Settlement
                && string.IsNullOrWhiteSpace(definition.TargetId))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Settlement reputation '{definition.ReputationDefinitionId}' is missing targetId.");
            }

            return CCS_SurvivalValidationResult.Pass($"Reputation '{definition.ReputationDefinitionId}' validated.");
        }
    }
}
