using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeHarvestProfile
// CATEGORY: Modules / Wildlife / Runtime / Profiles
// PURPOSE: Catalog of frontier wildlife harvest definitions for validation and lookup.
// PLACEMENT: Assets/CCS/Survival/Profiles/Wildlife/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.3.2 frontier hunting foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    [CreateAssetMenu(
        fileName = "CCS_WildlifeHarvestProfile",
        menuName = "CCS/Survival/Wildlife/Wildlife Harvest Profile")]
    public sealed class CCS_WildlifeHarvestProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Tooltip("Registered wildlife harvest definitions.")]
        [SerializeField] private List<CCS_WildlifeHarvestDefinition> harvestDefinitions =
            new List<CCS_WildlifeHarvestDefinition>();

        #endregion

        #region Properties

        public IReadOnlyList<CCS_WildlifeHarvestDefinition> HarvestDefinitions => harvestDefinitions;

        #endregion

        #region Public Methods

        public bool TryGetByWildlifeId(string wildlifeId, out CCS_WildlifeHarvestDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(wildlifeId) || harvestDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < harvestDefinitions.Count; index++)
            {
                CCS_WildlifeHarvestDefinition candidate = harvestDefinitions[index];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.WildlifeDefinition != null
                    && candidate.WildlifeDefinition.WildlifeId == wildlifeId)
                {
                    definition = candidate;
                    return true;
                }

                if (candidate.HarvestDefinitionId == wildlifeId)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetByHarvestDefinitionId(string harvestDefinitionId, out CCS_WildlifeHarvestDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(harvestDefinitionId) || harvestDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < harvestDefinitions.Count; index++)
            {
                CCS_WildlifeHarvestDefinition candidate = harvestDefinitions[index];
                if (candidate != null && candidate.HarvestDefinitionId == harvestDefinitionId)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
