using CCS.Modules.Banking;
using CCS.Modules.Economy;
using CCS.Modules.Land;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_UpkeepValidationUtility
// CATEGORY: Modules / Upkeep / Runtime / Validation
// PURPOSE: Shared profile/content validation for upkeep bootstrap and runtime.
// PLACEMENT: Used by CCS_UpkeepService and editor validators.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 tax and upkeep foundation.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    public static class CCS_UpkeepValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_UpkeepProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Upkeep profile is null.");
            }

            CCS_UpkeepDefinition[] definitions = profile.UpkeepDefinitions;
            if (definitions == null || definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Upkeep profile has no upkeep definitions.");
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SurvivalValidationResult definitionResult = ValidateDefinition(definitions[index]);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }
            }

            if (!profile.TryGetDefaultLandClaimUpkeep(out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Upkeep profile default land claim id '{profile.DefaultLandClaimUpkeepDefinitionId}' was not found.");
            }

            return CCS_SurvivalValidationResult.Pass("Upkeep profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateDefinition(CCS_UpkeepDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Upkeep definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.UpkeepDefinitionId))
            {
                return CCS_SurvivalValidationResult.Fail("Upkeep definition is missing upkeepDefinitionId.");
            }

            if (string.IsNullOrWhiteSpace(definition.CurrencyId))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Upkeep '{definition.UpkeepDefinitionId}' is missing currencyId.");
            }

            if (definition.Amount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Upkeep '{definition.UpkeepDefinitionId}' has invalid amount.");
            }

            return CCS_SurvivalValidationResult.Pass($"Upkeep '{definition.UpkeepDefinitionId}' validated.");
        }

        public static CCS_SurvivalValidationResult ValidateBankingBinding(CCS_BankingService bankingService)
        {
            if (bankingService == null || !bankingService.IsInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Banking service is not ready for upkeep payments.");
            }

            return CCS_SurvivalValidationResult.Pass("Upkeep banking binding validated.");
        }

        public static CCS_SurvivalValidationResult ValidateCurrencyBinding(CCS_CurrencyService currencyService)
        {
            if (currencyService == null || !currencyService.IsInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Currency service is not ready for upkeep payments.");
            }

            if (!currencyService.TryGetCurrencyDefinition(CCS_UpkeepContentIds.TradeDollarsCurrencyId, out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Currency service is missing '{CCS_UpkeepContentIds.TradeDollarsCurrencyId}'.");
            }

            return CCS_SurvivalValidationResult.Pass("Upkeep currency binding validated.");
        }

        public static CCS_SurvivalValidationResult ValidateLandClaimBinding(CCS_LandClaimService landClaimService)
        {
            if (landClaimService == null || !landClaimService.IsInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Land claim service is not ready for upkeep registration.");
            }

            return CCS_SurvivalValidationResult.Pass("Upkeep land claim binding validated.");
        }
    }
}
