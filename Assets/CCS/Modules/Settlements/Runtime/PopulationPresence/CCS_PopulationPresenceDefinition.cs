using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceDefinition
// CATEGORY: Modules / Settlements / Runtime / PopulationPresence
// PURPOSE: Maps workforce category to bootstrap population presence anchor metadata.
// PLACEMENT: Serialized on CCS_PopulationPresenceProfile.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — NPC population placeholder foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_PopulationPresenceDefinition
    {
        public string anchorId = string.Empty;

        public string settlementId = string.Empty;

        public CCS_SettlementPopulationCategory workforceCategory = CCS_SettlementPopulationCategory.Unknown;

        public int minimumPopulationCount = 1;

        public int maxVisibleActors = 4;

        public float spawnRadius = 2.5f;

        public string businessId = string.Empty;

        public CCS_SettlementGrowthStage requiredGrowthStage = CCS_SettlementGrowthStage.Unknown;

        public string displayName = string.Empty;

        public string AnchorId => anchorId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? workforceCategory.ToString() : displayName;
    }
}
