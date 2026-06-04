using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ContractValidationUtility
// CATEGORY: Modules / Contracts / Runtime / Validation
// PURPOSE: Shared profile and definition validation for contracts bootstrap and runtime.
// PLACEMENT: Used by CCS_ContractService and editor validators.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public static class CCS_ContractValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_ContractProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Contract profile is null.");
            }

            CCS_ContractDefinition[] definitions = profile.ContractDefinitions;
            if (definitions == null || definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Contract profile has no contract definitions.");
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SurvivalValidationResult definitionResult = ValidateDefinition(definitions[index]);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }
            }

            if (string.IsNullOrWhiteSpace(profile.DefaultSettlementId))
            {
                return CCS_SurvivalValidationResult.Fail("Contract profile is missing defaultSettlementId.");
            }

            return CCS_SurvivalValidationResult.Pass("Contract profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateDefinition(CCS_ContractDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Contract definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.ContractId))
            {
                return CCS_SurvivalValidationResult.Fail("Contract definition is missing contractId.");
            }

            if (string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Contract '{definition.ContractId}' is missing displayName.");
            }

            CCS_ContractRequirement[] requirements = definition.Requirements;
            if (requirements == null || requirements.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Contract '{definition.ContractId}' has no requirements.");
            }

            for (int index = 0; index < requirements.Length; index++)
            {
                CCS_ContractRequirement requirement = requirements[index];
                if (requirement == null || string.IsNullOrWhiteSpace(requirement.ItemId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Contract '{definition.ContractId}' has an invalid requirement.");
                }

                if (requirement.Quantity <= 0)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Contract '{definition.ContractId}' requirement quantity must be positive.");
                }
            }

            CCS_ContractReward reward = definition.Reward;
            if (reward == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Contract '{definition.ContractId}' is missing reward configuration.");
            }

            if (reward.TradeDollars < 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Contract '{definition.ContractId}' trade dollar reward cannot be negative.");
            }

            return CCS_SurvivalValidationResult.Pass($"Contract '{definition.ContractId}' validated.");
        }
    }
}
