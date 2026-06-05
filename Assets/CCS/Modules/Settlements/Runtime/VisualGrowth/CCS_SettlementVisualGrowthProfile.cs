using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthProfile
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: Catalog of settlement visual growth anchors for validation and bootstrap.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/VisualGrowth/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — wired on CCS_SettlementVisualGrowthService.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementVisualGrowthProfile",
        menuName = "CCS/Survival/Settlements/Settlement Visual Growth Profile")]
    public sealed class CCS_SettlementVisualGrowthProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_SettlementVisualGrowthDefinition[] anchorDefinitions =
            Array.Empty<CCS_SettlementVisualGrowthDefinition>();

        public CCS_SettlementVisualGrowthDefinition[] AnchorDefinitions =>
            anchorDefinitions ?? Array.Empty<CCS_SettlementVisualGrowthDefinition>();

        public bool TryGetDefinition(string anchorId, out CCS_SettlementVisualGrowthDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return false;
            }

            CCS_SettlementVisualGrowthDefinition[] definitions = AnchorDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementVisualGrowthDefinition candidate = definitions[index];
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
