using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceProfile
// CATEGORY: Modules / Settlements / Runtime / PopulationPresence
// PURPOSE: Catalog of population presence anchors for validation and bootstrap.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/PopulationPresence/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — wired on CCS_PopulationPresenceService.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_PopulationPresenceProfile",
        menuName = "CCS/Survival/Settlements/Population Presence Profile")]
    public sealed class CCS_PopulationPresenceProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_PopulationPresenceDefinition[] anchorDefinitions =
            Array.Empty<CCS_PopulationPresenceDefinition>();

        public CCS_PopulationPresenceDefinition[] AnchorDefinitions =>
            anchorDefinitions ?? Array.Empty<CCS_PopulationPresenceDefinition>();

        public bool TryGetDefinition(string anchorId, out CCS_PopulationPresenceDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return false;
            }

            CCS_PopulationPresenceDefinition[] definitions = AnchorDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_PopulationPresenceDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.AnchorId, anchorId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
