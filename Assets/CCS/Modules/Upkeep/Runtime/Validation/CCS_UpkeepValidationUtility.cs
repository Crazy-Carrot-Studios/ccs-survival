using System.IO;
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
// NOTES: Milestone 2.5.0 tax and upkeep foundation; 2.5.1 release safety contracts.
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

        public static CCS_SurvivalValidationResult ValidateReleaseSafetyContracts(
            string upkeepServiceSourcePath,
            string saveServiceSourcePath,
            string bankingServiceSourcePath)
        {
            if (!File.Exists(upkeepServiceSourcePath))
            {
                return CCS_SurvivalValidationResult.Fail("CCS_UpkeepService source missing for release safety validation.");
            }

            string upkeepSource = File.ReadAllText(upkeepServiceSourcePath);
            if (!upkeepSource.Contains("TryRegisterLandClaimUpkeep")
                || !upkeepSource.Contains("entriesByTargetId.ContainsKey")
                || !upkeepSource.Contains("CaptureUpkeepState")
                || !upkeepSource.Contains("RestoreState")
                || !upkeepSource.Contains("ReconcileLandClaimEntries")
                || !upkeepSource.Contains("TryPayUpkeep")
                || !upkeepSource.Contains("CanDebitForUpkeep")
                || !upkeepSource.Contains("RemoveCurrency")
                || !upkeepSource.Contains("CCS_UpkeepState.Failed")
                || !upkeepSource.Contains("CCS_UpkeepTransactionResultType.InsufficientFunds"))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Upkeep service missing required registration, save/load, payment, or safe-failure contracts.");
            }

            if (!File.Exists(saveServiceSourcePath))
            {
                return CCS_SurvivalValidationResult.Fail("CCS_SaveService source missing for upkeep save/load validation.");
            }

            string saveSource = File.ReadAllText(saveServiceSourcePath);
            if (!saveSource.Contains("CaptureUpkeep")
                || !saveSource.Contains("ApplyUpkeep")
                || !saveSource.Contains("ReconcileLandClaimEntries"))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Save service missing upkeep capture, apply, or post-load reconcile paths.");
            }

            if (!File.Exists(bankingServiceSourcePath))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "CCS_BankingService source missing for upkeep bank debit validation.");
            }

            string bankingSource = File.ReadAllText(bankingServiceSourcePath);
            if (!bankingSource.Contains("TryDebitForUpkeep")
                || !bankingSource.Contains("CanDebitForUpkeep"))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Banking service missing upkeep debit contracts for bank-only upkeep payments.");
            }

            return CCS_SurvivalValidationResult.Pass(
                "Upkeep release safety contracts validated (register, save/load, bank/wallet pay, safe fail, reconcile).");
        }
    }
}
