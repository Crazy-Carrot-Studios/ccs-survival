using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementHousingProfile
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: Catalog of settlement housing definitions for validation and bootstrap.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/Housing/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — wired on CCS_SettlementHousingService.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementHousingProfile",
        menuName = "CCS/Survival/Settlements/Settlement Housing Profile")]
    public sealed class CCS_SettlementHousingProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_SettlementHousingDefinition[] housingDefinitions =
            Array.Empty<CCS_SettlementHousingDefinition>();

        public CCS_SettlementHousingDefinition[] HousingDefinitions =>
            housingDefinitions ?? Array.Empty<CCS_SettlementHousingDefinition>();

        public bool TryGetDefinition(string housingId, out CCS_SettlementHousingDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(housingId))
            {
                return false;
            }

            CCS_SettlementHousingDefinition[] definitions = HousingDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementHousingDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.HousingId, housingId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetDefinitionForSettlement(
            string settlementId,
            out CCS_SettlementHousingDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            CCS_SettlementHousingDefinition[] definitions = HousingDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementHousingDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
