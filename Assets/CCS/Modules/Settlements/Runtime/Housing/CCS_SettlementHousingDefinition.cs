using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementHousingDefinition
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: Profile entry describing settlement-owned housing capacity contribution.
// PLACEMENT: Serialized on CCS_SettlementHousingProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — activation gated by settlement growth stage.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_SettlementHousingDefinition
    {
        [SerializeField] private string housingId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private CCS_SettlementHousingType housingType = CCS_SettlementHousingType.Unknown;

        [SerializeField] private int capacityContribution = 1;

        [SerializeField] private CCS_SettlementGrowthStage requiredGrowthStage = CCS_SettlementGrowthStage.Outpost;

        [SerializeField] private CCS_SettlementPopulationCategory workforceAffinity =
            CCS_SettlementPopulationCategory.Unknown;

        public string HousingId => housingId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string AnchorId => anchorId ?? string.Empty;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? housingType.ToString() : displayName;

        public CCS_SettlementHousingType HousingType => housingType;

        public int CapacityContribution => capacityContribution;

        public CCS_SettlementGrowthStage RequiredGrowthStage => requiredGrowthStage;

        public CCS_SettlementPopulationCategory WorkforceAffinity => workforceAffinity;
    }
}
