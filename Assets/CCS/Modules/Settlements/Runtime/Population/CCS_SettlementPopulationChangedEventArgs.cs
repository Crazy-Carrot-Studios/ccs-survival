// =============================================================================
// SCRIPT: CCS_SettlementPopulationChangedEventArgs
// CATEGORY: Modules / Settlements / Runtime / Population
// PURPOSE: Event payload when settlement population metrics change.
// PLACEMENT: Raised by CCS_WorldSimulationService.SettlementPopulationChanged.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 population foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementPopulationChangedEventArgs
    {
        public CCS_SettlementPopulationChangedEventArgs(
            CCS_SettlementPopulationSnapshot snapshot,
            int previousTotalPopulation,
            bool totalPopulationChanged)
        {
            Snapshot = snapshot ?? CCS_SettlementPopulationSnapshot.Empty;
            PreviousTotalPopulation = previousTotalPopulation < 0 ? 0 : previousTotalPopulation;
            TotalPopulationChanged = totalPopulationChanged;
        }

        public CCS_SettlementPopulationSnapshot Snapshot { get; }

        public int PreviousTotalPopulation { get; }

        public bool TotalPopulationChanged { get; }
    }
}
