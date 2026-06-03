using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LoanProfile
// CATEGORY: Modules / Banking / Runtime / Profiles
// PURPOSE: Profile catalog for loan definitions referenced by bank services.
// PLACEMENT: Assets/CCS/Survival/Profiles/Banking/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Referenced from CCS_BankAccountProfile. Milestone 2.6.0.
// =============================================================================

namespace CCS.Modules.Banking
{
    [CreateAssetMenu(
        fileName = "CCS_LoanProfile",
        menuName = "CCS/Survival/Banking/Loan Profile")]
    public sealed class CCS_LoanProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_LoanDefinition[] loanDefinitions = Array.Empty<CCS_LoanDefinition>();
        [SerializeField] private string defaultLoanDefinitionId = CCS_BankingContentIds.FrontierSmallLoanDefinitionId;
        [SerializeField] private bool enableDebugLogging = true;

        public CCS_LoanDefinition[] LoanDefinitions => loanDefinitions ?? Array.Empty<CCS_LoanDefinition>();

        public string DefaultLoanDefinitionId => defaultLoanDefinitionId ?? string.Empty;

        public bool EnableDebugLogging => enableDebugLogging;

        public bool TryGetLoanById(string loanDefinitionId, out CCS_LoanDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(loanDefinitionId))
            {
                return false;
            }

            CCS_LoanDefinition[] definitions = LoanDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_LoanDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.LoanDefinitionId, loanDefinitionId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetDefaultLoan(out CCS_LoanDefinition definition)
        {
            return TryGetLoanById(DefaultLoanDefinitionId, out definition);
        }
    }
}
