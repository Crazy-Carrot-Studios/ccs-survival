using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceProfile
// CATEGORY: Modules / Settlements / Runtime / BusinessPresence
// PURPOSE: Catalog of business presence anchors for validation and bootstrap.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/BusinessPresence/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — wired on CCS_BusinessPresenceService.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_BusinessPresenceProfile",
        menuName = "CCS/Survival/Settlements/Business Presence Profile")]
    public sealed class CCS_BusinessPresenceProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_BusinessPresenceDefinition[] anchorDefinitions =
            Array.Empty<CCS_BusinessPresenceDefinition>();

        public CCS_BusinessPresenceDefinition[] AnchorDefinitions =>
            anchorDefinitions ?? Array.Empty<CCS_BusinessPresenceDefinition>();

        public bool TryGetDefinition(string anchorId, out CCS_BusinessPresenceDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return false;
            }

            CCS_BusinessPresenceDefinition[] definitions = AnchorDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_BusinessPresenceDefinition candidate = definitions[index];
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
