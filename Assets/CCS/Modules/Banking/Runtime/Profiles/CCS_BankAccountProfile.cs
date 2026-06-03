using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BankAccountProfile
// CATEGORY: Modules / Banking / Runtime / Profiles
// PURPOSE: Profile catalog for bank account definitions.
// PLACEMENT: Assets/CCS/Survival/Profiles/Banking/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Registered on CCS_SurvivalGameplayServiceHost. Milestone 2.4.0.
// =============================================================================

namespace CCS.Modules.Banking
{
    [CreateAssetMenu(
        fileName = "CCS_BankAccountProfile",
        menuName = "CCS/Survival/Banking/Bank Account Profile")]
    public sealed class CCS_BankAccountProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_BankAccountDefinition[] accountDefinitions = Array.Empty<CCS_BankAccountDefinition>();
        [SerializeField] private string defaultAccountDefinitionId = CCS_BankingContentIds.FrontierSavingsAccountDefinitionId;
        [SerializeField] private bool enableDebugLogging = true;

        public CCS_BankAccountDefinition[] AccountDefinitions =>
            accountDefinitions ?? Array.Empty<CCS_BankAccountDefinition>();

        public string DefaultAccountDefinitionId => defaultAccountDefinitionId ?? string.Empty;

        public bool EnableDebugLogging => enableDebugLogging;

        public bool TryGetAccountById(string accountDefinitionId, out CCS_BankAccountDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(accountDefinitionId))
            {
                return false;
            }

            CCS_BankAccountDefinition[] definitions = AccountDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_BankAccountDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.AccountDefinitionId, accountDefinitionId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
