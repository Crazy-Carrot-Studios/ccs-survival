using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LandClaimProfile
// CATEGORY: Modules / Land / Runtime / Profiles
// PURPOSE: Profile catalog for land claim definitions.
// PLACEMENT: Assets/CCS/Survival/Profiles/Land/
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Registered on CCS_SurvivalGameplayServiceHost. Milestone 2.3.0.
// =============================================================================

namespace CCS.Modules.Land
{
    [CreateAssetMenu(
        fileName = "CCS_LandClaimProfile",
        menuName = "CCS/Survival/Land/Land Claim Profile")]
    public sealed class CCS_LandClaimProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_LandClaimDefinition[] claimDefinitions = Array.Empty<CCS_LandClaimDefinition>();
        [SerializeField] private bool enableDebugLogging = true;

        public CCS_LandClaimDefinition[] ClaimDefinitions => claimDefinitions ?? Array.Empty<CCS_LandClaimDefinition>();

        public bool EnableDebugLogging => enableDebugLogging;

        public bool TryGetClaimById(string claimDefinitionId, out CCS_LandClaimDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(claimDefinitionId))
            {
                return false;
            }

            CCS_LandClaimDefinition[] definitions = ClaimDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_LandClaimDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.ClaimDefinitionId, claimDefinitionId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetClaimByDeedItemId(string itemId, out CCS_LandClaimDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            CCS_LandClaimDefinition[] definitions = ClaimDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_LandClaimDefinition candidate = definitions[index];
                if (candidate?.ClaimDeedItem != null
                    && string.Equals(candidate.ClaimDeedItem.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
