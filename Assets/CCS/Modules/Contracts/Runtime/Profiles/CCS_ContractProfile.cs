using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractProfile
// CATEGORY: Modules / Contracts / Runtime / Profiles
// PURPOSE: Profile catalog for frontier contract definitions.
// PLACEMENT: Assets/CCS/Survival/Profiles/Contracts/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Registered on CCS_SurvivalGameplayServiceHost. Milestone 3.0.0.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [CreateAssetMenu(
        fileName = "CCS_ContractProfile",
        menuName = "CCS/Survival/Contracts/Contract Profile")]
    public sealed class CCS_ContractProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private string defaultSettlementId =
            CCS_ContractContentIds.DefaultTradingPostSettlementId;

        [SerializeField] private string defaultCurrencyId =
            CCS_ContractContentIds.TradeDollarsCurrencyId;

        [SerializeField] private CCS_ContractDefinition[] contractDefinitions =
            Array.Empty<CCS_ContractDefinition>();

        [SerializeField] private CCS_DynamicContractProfile dynamicContractProfile;

        [SerializeField] private bool enableDebugLogging = true;

        public string DefaultSettlementId => defaultSettlementId ?? string.Empty;

        public string DefaultCurrencyId => defaultCurrencyId ?? string.Empty;

        public CCS_ContractDefinition[] ContractDefinitions =>
            contractDefinitions ?? Array.Empty<CCS_ContractDefinition>();

        public CCS_DynamicContractProfile DynamicContractProfile => dynamicContractProfile;

        public bool EnableDebugLogging => enableDebugLogging;

        public bool TryGetDefinitionById(string contractId, out CCS_ContractDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(contractId))
            {
                return false;
            }

            CCS_ContractDefinition[] definitions = ContractDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ContractDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.ContractId, contractId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
