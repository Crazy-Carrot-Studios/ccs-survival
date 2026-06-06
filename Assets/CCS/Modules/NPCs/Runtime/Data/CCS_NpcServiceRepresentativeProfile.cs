using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeProfile
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Catalog of business/service representative mappings per settlement.
// PLACEMENT: Assets/CCS/Survival/Profiles/NPCs/ServiceRepresentatives/
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — wired on CCS_NpcServiceRepresentativeService.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [CreateAssetMenu(
        fileName = "CCS_NpcServiceRepresentativeProfile",
        menuName = "CCS/Survival/NPCs/NPC Service Representative Profile")]
    public sealed class CCS_NpcServiceRepresentativeProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_NpcServiceRepresentativeDefinition[] representativeDefinitions =
            Array.Empty<CCS_NpcServiceRepresentativeDefinition>();

        public CCS_NpcServiceRepresentativeDefinition[] RepresentativeDefinitions =>
            representativeDefinitions ?? Array.Empty<CCS_NpcServiceRepresentativeDefinition>();

        public bool TryGetDefinitionForBusiness(
            string settlementId,
            string businessId,
            out CCS_NpcServiceRepresentativeDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return false;
            }

            CCS_NpcServiceRepresentativeDefinition[] definitions = RepresentativeDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcServiceRepresentativeDefinition candidate = definitions[index];
                if (candidate != null && candidate.MatchesBusiness(settlementId, businessId))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetDefinitionByRepresentativeId(
            string representativeId,
            out CCS_NpcServiceRepresentativeDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(representativeId))
            {
                return false;
            }

            CCS_NpcServiceRepresentativeDefinition[] definitions = RepresentativeDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcServiceRepresentativeDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.RepresentativeId, representativeId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
