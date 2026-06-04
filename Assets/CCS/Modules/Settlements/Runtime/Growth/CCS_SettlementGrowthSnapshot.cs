// =============================================================================
// SCRIPT: CCS_SettlementGrowthSnapshot
// CATEGORY: Modules / Settlements / Runtime / Growth
// PURPOSE: Runtime settlement growth record for services, HUD, and save payloads.
// PLACEMENT: Stored on settlement simulation state and exposed by settlement service.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementGrowthSnapshot
    {
        public static readonly CCS_SettlementGrowthSnapshot Empty = new CCS_SettlementGrowthSnapshot();

        public string SettlementId { get; set; } = string.Empty;

        public CCS_SettlementGrowthStage CurrentGrowthStage { get; set; }

        public CCS_SettlementGrowthStage PreviousGrowthStage { get; set; }

        public CCS_SettlementGrowthStage NextGrowthStage { get; set; }

        public float GrowthProgressPercent { get; set; }

        public int CompletedContractsCount { get; set; }

        public float Prosperity { get; set; }

        public float FoodSupplyHealthPercent { get; set; }

        public float IndustrialSupplyHealthPercent { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(SettlementId);
    }
}
