using CCS.Modules.Economy;
using CCS.Modules.Land;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_BankingValidationUtility
// CATEGORY: Modules / Banking / Runtime / Validation
// PURPOSE: Shared profile/content validation for banking bootstrap and runtime.
// PLACEMENT: Used by CCS_BankingService and editor validators.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.6.0 banking, land office, and loan foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    public static class CCS_BankingValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_BankAccountProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Bank account profile is null.");
            }

            CCS_BankAccountDefinition[] accounts = profile.AccountDefinitions;
            if (accounts == null || accounts.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Bank account profile has no account definitions.");
            }

            for (int index = 0; index < accounts.Length; index++)
            {
                CCS_SurvivalValidationResult definitionResult = ValidateAccountDefinition(accounts[index]);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }
            }

            if (!profile.TryGetAccountById(profile.DefaultAccountDefinitionId, out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Bank account profile default id '{profile.DefaultAccountDefinitionId}' was not found.");
            }

            if (profile.LoanProfile != null)
            {
                CCS_SurvivalValidationResult loanProfileResult = ValidateLoanProfile(profile.LoanProfile);
                if (!loanProfileResult.IsSuccess)
                {
                    return loanProfileResult;
                }
            }

            return CCS_SurvivalValidationResult.Pass("Bank account profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateLoanProfile(CCS_LoanProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Loan profile is null.");
            }

            CCS_LoanDefinition[] loans = profile.LoanDefinitions;
            if (loans == null || loans.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Loan profile has no loan definitions.");
            }

            for (int index = 0; index < loans.Length; index++)
            {
                CCS_SurvivalValidationResult definitionResult = ValidateLoanDefinition(loans[index]);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }
            }

            if (!profile.TryGetLoanById(profile.DefaultLoanDefinitionId, out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Loan profile default id '{profile.DefaultLoanDefinitionId}' was not found.");
            }

            return CCS_SurvivalValidationResult.Pass("Loan profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateLoanDefinition(CCS_LoanDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Loan definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.LoanDefinitionId))
            {
                return CCS_SurvivalValidationResult.Fail("Loan definition is missing loanDefinitionId.");
            }

            if (string.IsNullOrWhiteSpace(definition.CurrencyId))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Loan '{definition.LoanDefinitionId}' is missing currencyId.");
            }

            if (definition.PrincipalAmount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Loan '{definition.LoanDefinitionId}' principal must be greater than zero.");
            }

            if (definition.RepaymentAmount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Loan '{definition.LoanDefinitionId}' repayment must be greater than zero.");
            }

            if (definition.MaxActiveLoans <= 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Loan '{definition.LoanDefinitionId}' maxActiveLoans must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass($"Loan '{definition.LoanDefinitionId}' validated.");
        }

        public static CCS_SurvivalValidationResult ValidateAccountDefinition(CCS_BankAccountDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Bank account definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.AccountDefinitionId))
            {
                return CCS_SurvivalValidationResult.Fail("Bank account definition is missing accountDefinitionId.");
            }

            if (string.IsNullOrWhiteSpace(definition.CurrencyId))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Bank account '{definition.AccountDefinitionId}' is missing currencyId.");
            }

            return CCS_SurvivalValidationResult.Pass($"Bank account '{definition.AccountDefinitionId}' validated.");
        }

        public static CCS_SurvivalValidationResult ValidateCurrencyBinding(CCS_CurrencyService currencyService)
        {
            if (currencyService == null || !currencyService.IsInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Currency service is not ready for banking.");
            }

            if (!currencyService.TryGetCurrencyDefinition(CCS_BankingContentIds.TradeDollarsCurrencyId, out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Currency service is missing '{CCS_BankingContentIds.TradeDollarsCurrencyId}'.");
            }

            return CCS_SurvivalValidationResult.Pass("Banking currency binding validated.");
        }

        public static CCS_SurvivalValidationResult ValidateLandClaimBinding(CCS_LandClaimService landClaimService)
        {
            if (landClaimService == null || !landClaimService.IsInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Land claim service is not ready for land office display.");
            }

            return CCS_SurvivalValidationResult.Pass("Land office land claim binding validated.");
        }
    }
}
