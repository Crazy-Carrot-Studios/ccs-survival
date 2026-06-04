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

        public static CCS_SurvivalValidationResult ValidateServiceAccessProfile(CCS_ServiceAccessProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Service access profile is null. Run CCS_ServiceAccessFoundationBootstrapSetup.ExecuteBatch.");
            }

            CCS_ServiceAccessRule[] rules = profile.ServiceAccessRules;
            if (rules == null || rules.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Service access profile has no rules.");
            }

            for (int index = 0; index < rules.Length; index++)
            {
                CCS_SurvivalValidationResult ruleResult = ValidateServiceAccessRule(rules[index]);
                if (!ruleResult.IsSuccess)
                {
                    return ruleResult;
                }
            }

            return CCS_SurvivalValidationResult.Pass("Service access profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateServiceAccessRule(CCS_ServiceAccessRule rule)
        {
            if (rule == null)
            {
                return CCS_SurvivalValidationResult.Fail("Service access rule is null.");
            }

            if (string.IsNullOrWhiteSpace(rule.RuleId))
            {
                return CCS_SurvivalValidationResult.Fail("Service access rule is missing ruleId.");
            }

            if (string.IsNullOrWhiteSpace(rule.SettlementId))
            {
                return CCS_SurvivalValidationResult.Fail($"Service access rule '{rule.RuleId}' is missing settlementId.");
            }

            if (rule.Requirement == null)
            {
                return CCS_SurvivalValidationResult.Fail($"Service access rule '{rule.RuleId}' is missing requirement.");
            }

            return CCS_SurvivalValidationResult.Pass($"Service access rule '{rule.RuleId}' validated.");
        }

        public static CCS_SurvivalValidationResult ValidatePriceModifiers(CCS_ReputationProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Reputation profile is null.");
            }

            if (profile.ServiceAccessProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Reputation profile is missing serviceAccessProfile reference.");
            }

            if (profile.NeutralBuyPriceModifier <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Neutral buy price modifier must be greater than zero.");
            }

            if (profile.TrustedBuyPriceModifier <= 0f || profile.HonoredBuyPriceModifier <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Trusted and honored buy modifiers must be greater than zero.");
            }

            if (profile.DistrustedBuyPriceModifier <= 0f || profile.HostileBuyPriceModifier <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Distrusted and hostile buy modifiers must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass("Reputation price modifiers validated.");
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
