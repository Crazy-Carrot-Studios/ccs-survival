using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_UpkeepProfile
// CATEGORY: Modules / Upkeep / Runtime / Profiles
// PURPOSE: Profile catalog for upkeep and tax definitions.
// PLACEMENT: Assets/CCS/Survival/Profiles/Upkeep/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Registered on CCS_SurvivalGameplayServiceHost. Milestone 2.5.0.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    [CreateAssetMenu(
        fileName = "CCS_UpkeepProfile",
        menuName = "CCS/Survival/Upkeep/Upkeep Profile")]
    public sealed class CCS_UpkeepProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_UpkeepDefinition[] upkeepDefinitions = Array.Empty<CCS_UpkeepDefinition>();
        [SerializeField] private string defaultLandClaimUpkeepDefinitionId =
            CCS_UpkeepContentIds.FrontierHomesteadClaimTaxDefinitionId;
        [SerializeField] private bool enableDebugLogging = true;

        public CCS_UpkeepDefinition[] UpkeepDefinitions => upkeepDefinitions ?? Array.Empty<CCS_UpkeepDefinition>();

        public string DefaultLandClaimUpkeepDefinitionId => defaultLandClaimUpkeepDefinitionId ?? string.Empty;

        public bool EnableDebugLogging => enableDebugLogging;

        public bool TryGetDefinitionById(string upkeepDefinitionId, out CCS_UpkeepDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(upkeepDefinitionId))
            {
                return false;
            }

            CCS_UpkeepDefinition[] definitions = UpkeepDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_UpkeepDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.UpkeepDefinitionId, upkeepDefinitionId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetDefaultLandClaimUpkeep(out CCS_UpkeepDefinition definition)
        {
            return TryGetDefinitionById(DefaultLandClaimUpkeepDefinitionId, out definition);
        }
    }
}
