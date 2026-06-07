using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DynamicContractProfile
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Profile catalog for dynamic contract generation rules and limits.
// PLACEMENT: Assets/CCS/Survival/Profiles/Contracts/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 — linked from CCS_ContractProfile.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [CreateAssetMenu(
        fileName = "CCS_DynamicContractProfile",
        menuName = "CCS/Survival/Contracts/Dynamic Contract Profile")]
    public sealed class CCS_DynamicContractProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private int maxActiveGeneratedContractsPerSettlement = 3;

        [SerializeField] private int evaluationIntervalHours = 6;

        [SerializeField] private CCS_DynamicContractRule[] rules = Array.Empty<CCS_DynamicContractRule>();

        public int MaxActiveGeneratedContractsPerSettlement =>
            maxActiveGeneratedContractsPerSettlement < 1 ? 1 : maxActiveGeneratedContractsPerSettlement;

        public int EvaluationIntervalHours => evaluationIntervalHours < 1 ? 1 : evaluationIntervalHours;

        public CCS_DynamicContractRule[] Rules => rules ?? Array.Empty<CCS_DynamicContractRule>();

        public bool TryGetRuleById(string ruleId, out CCS_DynamicContractRule rule)
        {
            rule = null;
            if (string.IsNullOrWhiteSpace(ruleId))
            {
                return false;
            }

            CCS_DynamicContractRule[] profileRules = Rules;
            for (int index = 0; index < profileRules.Length; index++)
            {
                CCS_DynamicContractRule candidate = profileRules[index];
                if (candidate != null
                    && candidate.Enabled
                    && string.Equals(candidate.RuleId, ruleId, StringComparison.OrdinalIgnoreCase))
                {
                    rule = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
