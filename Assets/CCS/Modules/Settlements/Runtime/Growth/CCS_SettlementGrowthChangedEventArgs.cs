// =============================================================================
// SCRIPT: CCS_SettlementGrowthChangedEventArgs
// CATEGORY: Modules / Settlements / Runtime / Growth
// PURPOSE: Event payload when a settlement growth stage changes.
// PLACEMENT: Raised by CCS_SettlementService and CCS_WorldSimulationService.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementGrowthChangedEventArgs
    {
        public CCS_SettlementGrowthSnapshot Snapshot { get; set; } = CCS_SettlementGrowthSnapshot.Empty;

        public bool StageChanged { get; set; }
    }
}
