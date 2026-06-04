using System;
using CCS.Modules.Reputation;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessDefinition
// CATEGORY: Modules / Settlements / Runtime / Businesses
// PURPOSE: Activation thresholds for a settlement business archetype.
// PLACEMENT: Serialized on CCS_BusinessProfile.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — population, prosperity, growth stage, optional reputation gates.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_BusinessDefinition
    {
        public string businessId = string.Empty;

        public string displayName = string.Empty;

        public CCS_BusinessType businessType = CCS_BusinessType.Unknown;

        [Tooltip("Minimum settlement population required to activate. 0 disables the gate.")]
        public int minimumPopulation;

        [Tooltip("Minimum prosperity (0-100) required to activate.")]
        public float minimumProsperity;

        [Tooltip("Minimum CCS_SettlementGrowthStage (int) required to activate.")]
        public int minimumGrowthStage = (int)CCS_SettlementGrowthStage.Outpost;

        [Tooltip("-1 disables reputation gate. Otherwise settlement standing must meet this tier.")]
        public int minimumReputationTier = -1;

        public string BusinessId => businessId ?? string.Empty;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? businessType.ToString() : displayName;

        public CCS_SettlementGrowthStage MinimumGrowthStage =>
            Enum.IsDefined(typeof(CCS_SettlementGrowthStage), minimumGrowthStage)
                ? (CCS_SettlementGrowthStage)minimumGrowthStage
                : CCS_SettlementGrowthStage.Outpost;

        public bool HasReputationGate =>
            minimumReputationTier >= 0
            && Enum.IsDefined(typeof(CCS_ReputationTier), minimumReputationTier);

        public CCS_ReputationTier MinimumReputationTier =>
            HasReputationGate ? (CCS_ReputationTier)minimumReputationTier : CCS_ReputationTier.Neutral;
    }
}
