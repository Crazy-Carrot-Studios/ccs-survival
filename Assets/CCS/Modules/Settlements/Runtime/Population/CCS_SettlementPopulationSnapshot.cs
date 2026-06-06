// =============================================================================
// SCRIPT: CCS_SettlementPopulationSnapshot
// CATEGORY: Modules / Settlements / Runtime / Population
// PURPOSE: Query snapshot for population HUD, playtest, and service bridges.
// PLACEMENT: Built by CCS_SettlementPopulationUtility from simulation state.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.4.0 — includes housing capacity breakdown fields.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementPopulationSnapshot
    {
        public static readonly CCS_SettlementPopulationSnapshot Empty = new CCS_SettlementPopulationSnapshot();

        public string SettlementId { get; set; } = string.Empty;

        public int TotalPopulation { get; set; }

        public int PopulationCapacity { get; set; }

        public int BasePopulationCapacity { get; set; }

        public int HousingCapacityContribution { get; set; }

        public int ActiveHousingCount { get; set; }

        public string[] ActiveHousingNames { get; set; } = System.Array.Empty<string>();

        public float PopulationGrowthRate { get; set; }

        public float PopulationStability { get; set; }

        public int FarmerCount { get; set; }

        public int RancherCount { get; set; }

        public int MinerCount { get; set; }

        public int LumberWorkerCount { get; set; }

        public int MerchantCount { get; set; }

        public int LaborerCount { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(SettlementId);
    }
}
