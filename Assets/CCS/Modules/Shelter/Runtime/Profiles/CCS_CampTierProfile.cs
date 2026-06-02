using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CampTierProfile
// CATEGORY: Modules / Shelter / Runtime / Profiles
// PURPOSE: Ordered camp tier definitions with structure requirements for automatic evaluation.
// PLACEMENT: Assets/CCS/Survival/Profiles/Camp/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    [CreateAssetMenu(
        fileName = "CCS_CampTierProfile",
        menuName = "CCS/Survival/Shelter/Camp Tier Profile")]
    public sealed class CCS_CampTierProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_CampTierDefinition[] tierDefinitions = Array.Empty<CCS_CampTierDefinition>();

        public IReadOnlyList<CCS_CampTierDefinition> TierDefinitions => tierDefinitions;

        public bool TryGetTierDefinition(CCS_CampTier tier, out CCS_CampTierDefinition definition)
        {
            definition = null;
            if (tierDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < tierDefinitions.Length; index++)
            {
                CCS_CampTierDefinition candidate = tierDefinitions[index];
                if (candidate != null && candidate.CampTier == tier)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public CCS_CampTierDefinition[] GetTiersOrderedAscending()
        {
            if (tierDefinitions == null || tierDefinitions.Length == 0)
            {
                return Array.Empty<CCS_CampTierDefinition>();
            }

            List<CCS_CampTierDefinition> copy = new List<CCS_CampTierDefinition>(tierDefinitions.Length);
            for (int index = 0; index < tierDefinitions.Length; index++)
            {
                if (tierDefinitions[index] != null)
                {
                    copy.Add(tierDefinitions[index]);
                }
            }

            copy.Sort((left, right) => ((int)left.CampTier).CompareTo((int)right.CampTier));
            return copy.ToArray();
        }
    }
}
