using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_IndustryProfile
// CATEGORY: Modules / Industry / Runtime / Profiles
// PURPOSE: Catalog of frontier industry processing and blacksmith recipes.
// PLACEMENT: Assets/CCS/Survival/Profiles/Industry/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Industry
{
    [CreateAssetMenu(
        fileName = "CCS_IndustryProfile",
        menuName = "CCS/Survival/Industry/Industry Profile")]
    public sealed class CCS_IndustryProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_IndustryDefinition[] processDefinitions = System.Array.Empty<CCS_IndustryDefinition>();
        [SerializeField] private CCS_BlacksmithRecipeDefinition[] blacksmithRecipes = System.Array.Empty<CCS_BlacksmithRecipeDefinition>();
        [SerializeField] private CCS_IndustryDefinition coalPlaceholder;
        [SerializeField] private CCS_IndustryDefinition sulfurPlaceholder;
        [SerializeField] private CCS_IndustryDefinition saltpeterPlaceholder;

        public IReadOnlyList<CCS_IndustryDefinition> ProcessDefinitions => processDefinitions;

        public IReadOnlyList<CCS_BlacksmithRecipeDefinition> BlacksmithRecipes => blacksmithRecipes;

        public CCS_IndustryDefinition CoalPlaceholder => coalPlaceholder;

        public CCS_IndustryDefinition SulfurPlaceholder => sulfurPlaceholder;

        public CCS_IndustryDefinition SaltpeterPlaceholder => saltpeterPlaceholder;

        public bool TryGetProcessById(string processId, out CCS_IndustryDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(processId) || processDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < processDefinitions.Length; index++)
            {
                CCS_IndustryDefinition candidate = processDefinitions[index];
                if (candidate != null
                    && string.Equals(candidate.ProcessId, processId, System.StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
